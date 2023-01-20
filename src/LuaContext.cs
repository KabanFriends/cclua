using CCLua.LuaObjects;
using CCLua.LuaObjects.Suppliers;
using MCGalaxy;
using MCGalaxy.Network;
using MCGalaxy.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLua;
using NLua.Event;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace CCLua
{
    public class LuaContext
    {
        public bool stopped;

        public Lua lua;
        public LuaConfiguration config;
        public LuaStaticMethodCaller caller;

        public Level level;

        public List<Thread> threads;

        public Dictionary<string, LuaPlayer> luaPlayers;
        public Dictionary<string, LuaTable> particleData;
        public Dictionary<string, byte> particleIds;

        public JObject dataJson;

        public Player currentPlayer;

        // Used for passing objects to lua
        public object[] obj;

        public string error;
        private long lastTimestamp;
        private long lastInstantTime;
        private int instructionCount;
        private bool doExecutionCheck;

        private bool saveQueued;
        private bool saving;

        public LuaContext(Level level)
        {
            this.level = level;
        }

        public void LoadLua()
        {
            lock (this)
            {
                string luaPath = Constants.CCLUA_BASE_DIR + Constants.SCRIPT_DIR + level.name + ".lua";
                string dataPath = Constants.CCLUA_BASE_DIR + Constants.STORAGE_DIR + level.name + ".dat";

                obj = new object[5];
                lua = new Lua();
                caller = new LuaStaticMethodCaller(this);
                threads = new List<Thread>();
                luaPlayers = new Dictionary<string, LuaPlayer>();
                particleData = new Dictionary<string, LuaTable>();
                particleIds = new Dictionary<string, byte>();

                lua.State.Encoding = Encoding.UTF8;

                string code = File.ReadAllText(luaPath);

                lastTimestamp = Environment.TickCount;

                SandboxUtil.SetupEnvironment(this);

                if (File.Exists(dataPath))
                {
                    byte[] dataBytes;
                    dataBytes = File.ReadAllBytes(dataPath);

                    string json = ZstdUtil.DecompressFromBytes(dataBytes);
                    dataJson = JObject.Parse(json);
                }
                else
                {
                    dataJson = new JObject();
                }

                try
                {
                    obj[0] = code;
                    SetExecutionCheck(true);
                    lua.DoString($"sandbox.run(context.obj[0], {{source = '{Path.GetFileName(luaPath)}'}})");
                    SetExecutionCheck(false);
                }
                catch (Exception e)
                {
                    ReportError(e.Message, null, true);
                    return;
                }

                Server.MainScheduler.QueueRepeat(delegate (SchedulerTask task)
                {
                    if (!stopped)
                    {
                        if (level != Server.mainLevel)
                        {
                            Logger.Log(LogType.SystemActivity, "cclua: Auto-saving data for level " + level.name);
                        }
                        SaveDataAsync();
                    }
                    else
                    {
                        task.Repeating = false;
                    }
                }, null, TimeSpan.FromSeconds(Constants.DATA_AUTOSAVE_SECONDS));
            }
        }

        public void CheckExecution(object sender, DebugHookEventArgs args)
        {
            if (!doExecutionCheck)
            {
                return;
            }

            long now = Environment.TickCount;
            long diff = now - lastTimestamp;
            long nanoDiff = TimeUtil.GetNanoseconds() - lastInstantTime;

            instructionCount += config.instructionsPerExecutionCheck;

            if (instructionCount > config.instructionLimit)
            {
                error = $"Instruction limit exceeded! (Line {args.LuaDebug.CurrentLine})";
                lua.State.Error(error);
                Stop();
            }

            if (nanoDiff > config.instantExecutionTimeNanos)
            {
                error = $"The code took too long to execute! This may have been caused by an infinite loop. (Line {args.LuaDebug.CurrentLine})";
                lua.State.Error(error);
                Stop();
            }

            if (diff > config.executionCheckTimeMs)
            {
                lastTimestamp = now;
                instructionCount = 0;
            }
        }

        public void SetExecutionCheck(bool enabled)
        {
            if (stopped) return;

            doExecutionCheck = enabled;

            if (enabled)
            {
                lastInstantTime = TimeUtil.GetNanoseconds();
            }
        }

        public void Stop()
        {
            if (stopped) return;
            stopped = true;

            for (int i = threads.Count - 1; i >= 0; i--)
            {
                threads[i].Interrupt();
            }

            Logger.Log(LogType.SystemActivity, "cclua: Saving data for level " + level.name);
            SaveDataAsync();

            foreach (Player p in level.players)
            {
                ResetPlayer(p);
            }

            lock (lua)
            {
                lua.Dispose();
            }

            ShowStoppedAll();
        }

        public void WaitForLua(Action action)
        {
            lock (lua)
            {
                if (stopped) return;

                action();

                lua.DoString("collectgarbage()");
            }
        }

        public void Print(string text)
        {
            Logger.Log(LogType.PlayerChat, "LUA DEBUG: " + text);
        }

        public void Call(string function, params object[] args)
        {
            CallByPlayer(function, null, args);
        }

        public void CallByPlayer(string function, Player player, params object[] args)
        {
            if (stopped) return;

            WaitForLua(delegate
            {
                RawCall(function, player, args);
            });
        }

        //Calls a lua function without waiting for lua context to be available.
        //Only call this when you are sure that the lua context is available.
        public void RawCall(string function, Player player, params object[] args)
        {
            LuaPlayer lp = null;
            if (player != null)
            {
                lp = GetLuaPlayer(player.truename);
                if (lp == null || lp.quit)
                {
                    return;
                }
            }

            try
            {
                obj[0] = function;
                obj[1] = player;
                obj[2] = args;

                currentPlayer = player;
                SetExecutionCheck(true);

                object[] result = lua.DoString(@"
local func = context.obj[0]
local player = context.obj[1]
local rawArgs = context.obj[2]

if env[func] ~= nil and type(env[func]) == 'function' then
    local args = {}
    if rawArgs.Length > 0 then
        for j = 1, rawArgs.Length do
            if context:IsObjectSupplier(rawArgs[j - 1]) then
                args[j] = rawArgs[j - 1]:CallSupply(context)
            else
                args[j] = rawArgs[j - 1]
            end
        end
    end

    local co = coroutine.create(function()
        env[func](table.unpack(args))
    end)

    return co
end
");
                if (result.Length > 0)
                {
                    RunLuaCoroutine(result[0], lp);
                }
            } catch (Exception e)
            {
                ReportError(e.Message, null, true);
            } finally
            {
                currentPlayer = null;
                SetExecutionCheck(false);
            }
        }

        public void RunLuaCoroutine(object coroutine, LuaPlayer lp)
        {
            currentPlayer = lp?.player;
            SetExecutionCheck(true);

            obj[0] = coroutine;
            object[] result = lua.DoString(@"
local co = context.obj[0]
local success, result = coroutine.resume(co)
local status = coroutine.status(co)
return success, result, status
");

            SetExecutionCheck(false);
            currentPlayer = null;

            bool success = (bool)result[0];
            string status = (string)result[2];

            if (success)
            {
                int wait = Convert.ToInt32(result[1]);
                if (status != "dead" && wait >= 0)
                {
                    Schedule(coroutine, wait, lp);
                }
            }
            else
            {
                ReportError(result[1].ToString(), lp?.player, false);
            }
        }

        public bool IsObjectSupplier(object obj)
        {
            return obj is LuaObjectSupplier;
        }

        public void Schedule(object coroutine, int waitMilliseconds, LuaPlayer lp = null)
        {
            ThreadStart ts = new ThreadStart(delegate
            {
                try
                {
                    Thread.Sleep(waitMilliseconds);

                    WaitForLua(delegate
                    {
                        if (lp != null && lp.quit)
                        {
                            return;
                        }

                        RunLuaCoroutine(coroutine, lp);
                    });

                    lock (threads)
                    {
                        threads.Remove(Thread.CurrentThread);
                    }
                    
                }
                catch (ThreadInterruptedException e)
                {
#if DEBUG
                    Print("Sleeping thread stopped (" + Thread.CurrentThread.ManagedThreadId + ")");
#endif
                }
                catch (Exception e)
                {
                    ReportError(e.ToString(), lp?.player, false);
                }
            });

            Thread t = new Thread(ts);
            t.Start();

            lock (threads)
            {
                threads.Add(t);
            }
        }

        public void ReportError(string error, Player player, bool stopIfGlobal)
        {
#if DEBUG
            Logger.Log(LogType.Error, "Error: " + error + " (Player: " + player + ")");
#endif

            if (player == null)
            {
                if (stopIfGlobal)
                {
                    this.error = FormatError(error);
                    Stop();
                    return;
                }

                foreach (Player p in level.players)
                {
                    p.Message("&eLua error:");
                    p.Message("&c" + FormatError(error));
                }
            } else
            {
                if (player.level == level)
                {
                    player.Message("&eLua error:");
                    player.Message("&c" + FormatError(error));
                }
            }
        }

        public string FormatError(string error)
        {
            Regex pattern = new Regex("^\\[string \"chunk\"\\]:(?:[0-9])+?:[ ]?(.*)$");
            return pattern.Replace(error, "$1");
        }

        public void ShowStopped(Player player)
        {
            if (stopped)
            {
                if (error != null)
                {
                    player.Message("&eLua execution in this map is blocked due to an error!");
                    player.Message("&c" + error);
                }
            }
        }

        public void ShowStoppedAll()
        {
            foreach (Player p in level.players)
            {
                ShowStopped(p);
            }
        }

        public void HandlePlayerJoin(Player p)
        {
            if (!CCLuaPlugin.usernameMap.ContainsKey(p.truename))
            {
                CCLuaPlugin.usernameMap.Add(p.truename, p.name);
            }

            if (luaPlayers.ContainsKey(p.truename)) return;

            ShowStopped(p);
            if (!stopped)
            {
                lock (lua)
                {
                    LuaPlayer lp = new LuaPlayer(p);
                    lp.CreateLuaTable(this);

                    luaPlayers.Add(p.truename, lp);

                    SendAllParticles(p);
                    RawCall("onPlayerJoin", p, new LuaSimplePlayerEventSupplier(new SimplePlayerEvent(p)));
                }
            }
        }

        public void HandlePlayerLeave(Player p)
        {
            if (!luaPlayers.ContainsKey(p.truename)) return;

            lock (lua)
            {
                RawCall("onPlayerLeave", p, new LuaSimplePlayerEventSupplier(new SimplePlayerEvent(p)));
            }

            ResetPlayer(p);

            luaPlayers[p.truename].quit = true;
            luaPlayers.Remove(p.truename);
        }

        public void ResetPlayer(Player p)
        {
            PlayerData data = GetLuaPlayer(p.truename)?.data;
            if (data != null)
            {
                if (LevelUtil.IsOsLevel(level))
                {
                    p.UpdateModel(data.model);
                }
                p.Send(Packet.ClickDistance(160));
                p.SendCpeMessage(CpeMessageType.Announcement, "");
                p.SendCpeMessage(CpeMessageType.BigAnnouncement, "");
                p.SendCpeMessage(CpeMessageType.SmallAnnouncement, "");
                p.SendCpeMessage(CpeMessageType.Status1, "");
                p.SendCpeMessage(CpeMessageType.Status2, "");
                p.SendCpeMessage(CpeMessageType.Status3, "");
                p.SendCpeMessage(CpeMessageType.BottomRight1, "");
                p.SendCpeMessage(CpeMessageType.BottomRight2, "");
                p.SendCpeMessage(CpeMessageType.BottomRight3, "");

                foreach (Hotkey key in data.hotkeys.Values)
                {
                    PlayerUtil.UndefineHotkey(p, key);
                }
            }
        }

        public bool IsPlayerInLevel(Player p)
        {
            if (stopped) return false;

            LuaPlayer lp = GetLuaPlayer(p.truename);
            if (lp == null) return false;
            if (lp.quit) return false;
            return true;
        }

        public LuaPlayer GetLuaPlayer(string name)
        {
            if (stopped) return null;

            LuaPlayer lp;
            return luaPlayers.TryGetValue(name, out lp) ? lp : null;
        }

        public long GetCurrentTimeMillis()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public void SendAllParticles(Player p)
        {
            foreach (string name in particleIds.Keys)
            {
                SendParticle(p, name);
            }
        }

        public void SendParticle(Player p, string name)
        {
            LuaTable particle = particleData[name];
            byte id = particleIds[name];
            PlayerUtil.DefineParticle(p, id, particle);
        }

        public void SaveData()
        {
            if (saving)
            {
                saveQueued = true;
                return;
            }

            saving = true;

            try
            {
                string dataPath = Constants.CCLUA_BASE_DIR + Constants.STORAGE_DIR + level.name + ".dat";

                string json = dataJson.ToString(Formatting.None);
                byte[] dataBytes = ZstdUtil.CompressToBytes(json);

                if (dataBytes.LongLength > config.storageMaxSize)
                {
                    foreach (Player p in level.getPlayers())
                    {
                        p.Message("&cFailed to save to the data storage; total data size exceeds the storage limit!");
                    }
                }
                else
                {
                    File.WriteAllBytes(dataPath, dataBytes);
                }
            } catch (Exception e)
            {
                Logger.LogError(e);
            }

            saving = false;

            if (saveQueued)
            {
                saveQueued = false;
                SaveData(); //run the queued save so there is no data lost
            }
        }

        public void SaveDataAsync()
        {
            ThreadStart ts = delegate
            {
                SaveData();
            };

            new Thread(ts).Start();
        }
    }
}
