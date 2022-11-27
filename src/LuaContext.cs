using CCLua.LuaObjects;
using CCLua.LuaObjects.Suppliers;
using MCGalaxy;
using MCGalaxy.Network;
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

        public LuaConfiguration config;

        public Lua lua;

        public Level level;

        public LuaStaticMethodCaller caller;

        public Dictionary<string, PlayerData> playerData;

        public Dictionary<string, LuaTable> luaPlayers;

        public string currentPlayerName;

        public string error;

        private long lastTimestamp;

        private long lastInstantTime;

        private int instructionCount;

        private bool doExecutionCheck;

        private AutoResetEvent doTask = new AutoResetEvent(false);

        private AutoResetEvent doLuaLoop = new AutoResetEvent(false);

        private int taskCount;

        public LuaContext(Level level)
        {
            this.level = level;
        }

        public void LoadLua(string path)
        {
            lua = new Lua();
            caller = new LuaStaticMethodCaller(this);
            playerData = new Dictionary<string, PlayerData>();
            luaPlayers = new Dictionary<string, LuaTable>();

            lua.State.Encoding = Encoding.UTF8;

            string code = File.ReadAllText(path);

            lastTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            SandboxUtil.SetupEnvironment(this);

            try
            {
                SetExecutionCheck(true);
                lua.DoString($"sandbox.run([[{code}]], {{source = '{Path.GetFileName(path)}'}})");
                SetExecutionCheck(false);
            }
            catch (Exception e)
            {
                error = FormatError(e.Message);
                Stop();
                return;
            }

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
                        }

                        if (stopped)
                        {
                            break;
                        }

                        lua.DoString(@"
local i = 1
while i <= #schedules do
    sch = schedules[i]
    if sch.player ~= nil and context:GetLuaPlayer(sch.player) == nil then
        table.remove(schedules, i)
        goto skip
    end
    if context:GetCurrentTimeMillis() > sch.sleepUntil then
        context.currentPlayerName = sch.player;
        context:SetExecutionCheck(true)
        local success, result = coroutine.resume(sch.thread)
        context:SetExecutionCheck(false)
        context.currentPlayerName = nil;
        if success then
            if coroutine.status(sch.thread) ~= 'dead' and type(result) == 'number' and result >= 0 then
                schedules[i].sleepUntil = context:GetCurrentTimeMillis() + result
            else
                table.remove(schedules, i)
                goto skip
            end
        else
            context:ReportError(tostring(result), sch.player)
            if context.stopped then
                break
            end
            table.remove(schedules, i)
            goto skip
        end
    end
    i = i + 1
    ::skip::
end
");
                    }
                } catch (Exception e)
                {
                    error = FormatError(e.Message);
                    Stop();
                    
                    doTask.Set(); //do not make the main thread stuck
                }

                lua.Close();
            };

            Thread t = new Thread(ts);
            t.Start();
        }

        public void CheckExecution(object sender, DebugHookEventArgs args)
        {
            if (!doExecutionCheck)
            {
                return;
            }

            long diff = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - lastTimestamp;
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
                lastTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
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

            foreach (Player p in level.players)
            {
                ResetPlayer(p);
            }

            ShowStoppedAll();
        }

        public void WaitForLua(Action action)
        {
            if (stopped) return;

            taskCount++;
            doTask.WaitOne();
            if (stopped) return;

            try
            {
                action();
            } catch (Exception e)
            {
                Logger.LogError(e);
            }

            if (taskCount > 0) taskCount--;
            doLuaLoop.Set();
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
                RawCallByPlayer(function, player, args);
            });
        }

        //Calls a lua function without waiting for lua context to be available.
        //Only call this when you are sure that the lua context is available.
        public void RawCallByPlayer(string function, Player player, params object[] args)
        {
            LuaCall call = new LuaCall(function, player, args);
            lua["call"] = call;

            lua.DoString(@"
local func = call.functionName
local player = call.playerName

if env[func] ~= nil and type(env[func]) == 'function' then
    local args = {}
    if call.args.Length > 0 then
        for j = 1, call.args.Length do
            if call:IsObjectSupplier(j - 1) then
                args[j] = call.args[j - 1]:CallSupply(context.lua)
            else
                args[j] = call.args[j - 1]
            end
        end
    end

    local co = coroutine.create(function()
        env[func](table.unpack(args))
    end)

    context.currentPlayerName = player;
    context:SetExecutionCheck(true)
    local success, result = coroutine.resume(co)
    context:SetExecutionCheck(false)
    context.currentPlayerName = nil;
    if success then
        if coroutine.status(co) ~= 'dead' and type(result) == 'number' and result >= 0 then
            local sch = {thread = co, player = player, sleepUntil = context:GetCurrentTimeMillis() + result}
            table.insert(schedules, sch)
        end
    else
        context:ReportError(tostring(result), player)
    end
end
");
        }

        public void ReportError(string error, string playerName)
        {
            if (playerName == null)
            {
                this.error = FormatError(error);
                Stop();
            } else
            {
                Player p = PlayerInfo.FindExact(CCLuaPlugin.usernameMap[playerName]);
                p.Message("&eLua error:");
                p.Message("&c" + error);
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
                    Logger.Log(LogType.Error, error);

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
            ShowStopped(p);
            if (!stopped)
            {
                playerData.Add(p.truename, new PlayerData(p));

                WaitForLua(delegate
                {
                    lua["csPlayer"] = p;

                    LuaTable luaPlayer = (LuaTable)lua.DoString(@"
local p = {}
local meta = {}

meta.obj = csPlayer
meta.__tostring = function()
    return 'Player'
end

setmetatable(p, meta)
return p
")[0];

                    luaPlayers.Add(p.truename, luaPlayer);

                    RawCallByPlayer("onPlayerJoin", p, new LuaSimplePlayerEventSupplier(new SimplePlayerEvent(p)));
                });
            }
        }

        public void HandlePlayerLeave(Player p)
        {
            WaitForLua(delegate
            {
                RawCallByPlayer("onPlayerLeave", p, new LuaSimplePlayerEventSupplier(new SimplePlayerEvent(p)));

                ResetPlayer(p);

                playerData.Remove(p.truename);
                luaPlayers.Remove(p.truename);
            });
        }

        public void ResetPlayer(Player p)
        {
            PlayerData data = GetPlayerData(p);
            if (data != null)
            {
                p.UpdateModel(data.model);
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
            if (!playerData.ContainsKey(p.truename)) return null;
            return playerData[p.truename];
        }

        public LuaTable GetLuaPlayer(string name)
        {
            if (!luaPlayers.ContainsKey(name)) return null;
            return luaPlayers[name];
        }

        public long GetCurrentTimeMillis()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
