using CCLua.LuaObjects;
using CCLua.LuaObjects.Suppliers;
using MCGalaxy;
using MCGalaxy.Network;
using MCGalaxy.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLua;
using NLua.Event;
using NLua.Exceptions;
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

        public List<LuaSchedule> schedules;

        public Dictionary<string, PlayerData> playerData;
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

        private AutoResetEvent doTask = new AutoResetEvent(false);
        private AutoResetEvent doLuaLoop = new AutoResetEvent(false);

        private int taskCount;

        private Dictionary<int, int> taskRecursions;

        public LuaContext(Level level)
        {
            this.level = level;
        }

        public void LoadLua()
        {
            string luaPath = Constants.CCLUA_BASE_DIR + Constants.SCRIPT_DIR + level.name + ".lua";
            string dataPath = Constants.CCLUA_BASE_DIR + Constants.STORAGE_DIR + level.name + ".dat";

            obj = new object[5];
            lua = new Lua();
            caller = new LuaStaticMethodCaller(this);
            schedules = new List<LuaSchedule>();
            playerData = new Dictionary<string, PlayerData>();
            luaPlayers = new Dictionary<string, LuaPlayer>();
            particleData = new Dictionary<string, LuaTable>();
            particleIds = new Dictionary<string, byte>();
            taskRecursions = new Dictionary<int, int>();

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
            } else
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

            ThreadStart ts = delegate
            {
                try
                {
                    while (true)
                    {
                        if (taskCount > 0)
                        {
                            doTask.Set();
                            doLuaLoop.WaitOne();

                            if (stopped)
                            {
                                break;
                            }
                        }

                        if (stopped)
                        {
                            break;
                        }

                        int tick = Environment.TickCount;
                        for (int i = schedules.Count - 1; i >= 0; i--)
                        {
                            LuaSchedule sch = schedules[i];
                            if (sch.luaPlayer != null && sch.luaPlayer.quit)
                            {
                                schedules.RemoveAt(i);
                                continue;
                            }

                            if (tick > sch.waitUntil)
                            {
                                currentPlayer = sch.luaPlayer?.player;
                                SetExecutionCheck(true);
                                obj[0] = sch;
                                object[] result = lua.DoString(@"
local sch = context.obj[0]
local success, result = coroutine.resume(sch.coroutine)
local status = coroutine.status(sch.coroutine)
return success, result, status
");
                                currentPlayer = null;
                                SetExecutionCheck(false);

                                bool success = (bool)result[0];
                                string status = (string)result[2];

                                if (success)
                                {
                                    long wait = Convert.ToInt64(result[1]);
                                    if (status != "dead" && wait >= 0)
                                    {
                                        sch.waitUntil = tick + wait;
                                    } else
                                    {
                                        schedules.RemoveAt(i);
                                        continue;
                                    }
                                } else
                                {
                                    ReportError(result[1].ToString(), sch.luaPlayer?.player, false);
                                    if (stopped)
                                    {
                                        break;
                                    }
                                    schedules.RemoveAt(i);
                                }
                            }
                        }
                    }
                } catch (Exception e)
                {
                    if (!(e is LuaScriptException)) {
                        Logger.LogError(e);
                    }

                    ReportError(e.Message, null, true);
                    doTask.Set(); //do not make the main thread stuck
                }
                lua.Dispose();
                lua.Close();
            };

            new Thread(ts).Start();
        }

        public void CheckExecution(object sender, DebugHookEventArgs args)
        {
            lua.DoString("collectgarbage()");

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
                lua.State.Error($"Instruction limit exceeded! (Line {args.LuaDebug.CurrentLine})");
                Stop();
            }

            if (nanoDiff > config.instantExecutionTimeNanos)
            {
                lua.State.Error($"The code took too long to execute! This may have been caused by an infinite loop. (Line {args.LuaDebug.CurrentLine})");
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

            Logger.Log(LogType.SystemActivity, "cclua: Saving data for level " + level.name);
            SaveDataAsync();

            foreach (Player p in level.players)
            {
                ResetPlayer(p);
            }

            ShowStoppedAll();
        }

        public void WaitForLua(Action action)
        {
            if (stopped)
            {
                doLuaLoop.Set();
                return;
            }

            var threadId = Thread.CurrentThread.ManagedThreadId;

            if (!taskRecursions.ContainsKey(threadId))
            {
                taskRecursions[threadId] = 0;
            }

            if (taskRecursions[threadId] == 0)
            {
                taskCount++;
                doTask.WaitOne();
            }

            if (stopped)
            {
                doLuaLoop.Set();
                return;
            }

            taskRecursions[threadId]++;

            action();

            if (taskRecursions[threadId] > 0) taskRecursions[threadId]--;

            if (taskRecursions[threadId] == 0)
            {
                taskRecursions.Remove(threadId);
                taskCount--;

                if (taskCount > 0)
                {
                    doTask.Set();
                } else
                {
                    taskCount = 0;;
                    doLuaLoop.Set();
                }
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

            if (player != null && !luaPlayers.ContainsKey(player.truename)) return;

            WaitForLua(delegate
            {
                RawCallByPlayer(function, player, args);
            });
        }

        //Calls a lua function without waiting for lua context to be available.
        //Only call this when you are sure that the lua context is available.
        public void RawCallByPlayer(string function, Player player, params object[] args)
        {
            if (player != null && luaPlayers[player.truename].quit) return;

            try
            {
                obj[0] = function;
                obj[1] = player;
                obj[2] = args;

                lua.DoString(@"
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

    context.currentPlayer = player;
    context:SetExecutionCheck(true)
    local success, result = coroutine.resume(co)
    context:SetExecutionCheck(false)
    context.currentPlayer = nil;
    if success then
        if coroutine.status(co) ~= 'dead' and type(result) == 'number' and result >= 0 then
            local lp = nil
            if player ~= nil and type(player) == 'string' then
                lp = context:GetLuaPlayer(player.truename)
            end
            context:Schedule(co, result, lp)
        end
    else
        context:ReportError(tostring(result), player, false)
    end
end
");
            } catch (Exception e)
            {
                ReportError(e.Message, null, true);
            }
        }

        public bool IsObjectSupplier(object obj)
        {
            return obj is LuaObjectSupplier;
        }

        public void Schedule(object coroutine, long waitMilliseconds, LuaPlayer luaPlayer = null)
        {
            schedules.Add(new LuaSchedule(coroutine, waitMilliseconds, luaPlayer));
        }

        public void ReportError(string error, Player player, bool stopIfGlobal)
        {
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
                playerData.Add(p.truename, new PlayerData(p));

                WaitForLua(delegate
                {
                    LuaPlayer lp = new LuaPlayer(p);
                    lp.CreateLuaTable(this);

                    luaPlayers.Add(p.truename, lp);

                    SendAllParticles(p);
                    RawCallByPlayer("onPlayerJoin", p, new LuaSimplePlayerEventSupplier(new SimplePlayerEvent(p)));
                });
            }
        }

        public void HandlePlayerLeave(Player p)
        {
            if (!luaPlayers.ContainsKey(p.truename)) return;

            WaitForLua(delegate
            {
                RawCallByPlayer("onPlayerLeave", p, new LuaSimplePlayerEventSupplier(new SimplePlayerEvent(p)));
            });

            luaPlayers[p.truename].quit = true;

            playerData.Remove(p.truename);
            luaPlayers.Remove(p.truename);

            ResetPlayer(p);
        }

        public void ResetPlayer(Player p)
        {
            PlayerData data = GetPlayerData(p);
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

        public PlayerData GetPlayerData(Player p)
        {
            PlayerData data;
            return playerData.TryGetValue(p.truename, out data) ? data : null;
        }

        public LuaPlayer GetLuaPlayer(string name)
        {
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
