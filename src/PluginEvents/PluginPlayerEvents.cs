using CCLua.LuaObjects.Suppliers;
using CCLua.LuaObjects;
using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Maths;
using System.Security.Policy;

namespace CCLua.PluginEvents
{
    public static class PluginPlayerEvents
    {
        public static void OnJoiningLevel(Player p, Level nextLevel, ref bool canJoin)
        {
            if (!canJoin) return;

            if (LevelHandler.HasLuaContext(p.level))
            {
                LevelHandler.GetContextByLevel(p.level).HandlePlayerLeave(p);
            }
        }

        public static void OnJoinedLevel(Player p, Level prevLevel, Level level, ref bool announce)
        {
            if (LevelHandler.HasLuaContext(level))
            {
                LevelHandler.GetContextByLevel(level).HandlePlayerJoin(p);
            }
        }

        public static void OnPlayerFinishConnecting(Player p)
        {
            if (!CCLuaPlugin.usernameMap.ContainsKey(p.truename))
            {
                CCLuaPlugin.usernameMap.Add(p.truename, p.name);
            }
        }

        public static void OnPlayerDisconnect(Player p, string reason)
        {
            if (LevelHandler.HasLuaContext(p.level))
            {
                LevelHandler.GetContextByLevel(p.level).HandlePlayerLeave(p);
            }

            p.Extras.Remove("cclua_upload_url");
            CCLuaPlugin.usernameMap.Remove(p.truename);
        }

        public static void OnPlayerMove(Player p, Position next, byte yaw, byte pitch, ref bool cancel)
        {
            if (LevelHandler.HasLuaContext(p.level))
            {
                LuaContext context = LevelHandler.GetContextByLevel(p.level);
                if (context == null) return;

                PlayerData data = context.GetPlayerData(p);

                if (p.Pos != next || p.Rot.RotY != yaw || p.Rot.HeadX != pitch)
                {
                    context.CallByPlayer("onPlayerMove", p, new LuaPlayerMoveEventSupplier(new PlayerMoveEvent(p, next, yaw, pitch)));
                }
            }
        }

        public static void OnPlayerClick(Player p, MouseButton btn, MouseAction action, ushort yaw, ushort pitch, byte entityID, ushort x, ushort y, ushort z, TargetBlockFace face)
        {
            if (LevelHandler.HasLuaContext(p.level))
            {
                LuaContext context = LevelHandler.GetContextByLevel(p.level);
                var supplier = new LuaPlayerClickEventSupplier(new PlayerClickEvent(p, yaw, pitch, entityID, x, y, z, face));

                if (action == MouseAction.Pressed)
                {
                    if (btn == MouseButton.Right)
                    {
                        context.CallByPlayer("onPlayerPressRightClick", p, supplier);
                    }
                    else if (btn == MouseButton.Left)
                    {
                        context.CallByPlayer("onPlayerPressLeftClick", p, supplier);
                    }
                    else if (btn == MouseButton.Middle)
                    {
                        context.CallByPlayer("onPlayerPressMiddleClick", p, supplier);
                    }
                }
                else if (action == MouseAction.Released)
                {
                    if (btn == MouseButton.Right)
                    {
                        context.CallByPlayer("onPlayerReleaseRightClick", p, supplier);
                    }
                    else if (btn == MouseButton.Left)
                    {
                        context.CallByPlayer("onPlayerReleaseLeftClick", p, supplier);
                    }
                    else if (btn == MouseButton.Middle)
                    {
                        context.CallByPlayer("onPlayerReleaseMiddleClick", p, supplier);
                    }
                }
            }
        }

        public static void OnPlayerChat(Player p, string message)
        {
            if (LevelHandler.HasLuaContext(p.level))
            {
                LuaContext context = LevelHandler.GetContextByLevel(p.level);

                if (message.StartsWith("$"))
                {
                    string input = message.Substring(1);
                    OnPlayerCommandEvent.Call(p, "input", input, new CommandData());
                    p.cancelchat = true;
                    p.cancelcommand = false;
                    return;
                }

                var ev = new PlayerChatEvent(p, message);
                context.CallByPlayer("onPlayerChat", p, new LuaPlayerChatEventSupplier(ev));

                if (ev.cancelState == CancelState.CANCELLED)
                {
                    p.cancelchat = true;
                }
            }
        }

        public static void OnBlockChanging(Player p, ushort x, ushort y, ushort z, ushort block, bool placing, ref bool cancel)
        {
            if (p == null) return;

            if (LevelHandler.HasLuaContext(p.level))
            {
                LuaContext context = LevelHandler.GetContextByLevel(p.level);

                ushort oldBlock = p.level.GetBlock(x, y, z);

                if (placing)
                {
                    var ev = new PlayerBlockChangeEvent(p, x, y, z, block);
                    context.CallByPlayer("onPlayerPlaceBlock", p, new LuaPlayerBlockChangeEventSupplier(ev));
                    
                    if (ev.cancelState == CancelState.CANCELLED)
                    {
                        cancel = true;
                        p.SendBlockchange(x, y, z, oldBlock);
                    }
                } else
                {
                    if (oldBlock == 0) return;

                    var ev = new PlayerBlockChangeEvent(p, x, y, z, oldBlock);
                    context.CallByPlayer("onPlayerBreakBlock", p, new LuaPlayerBlockChangeEventSupplier(ev));

                    if (ev.cancelState == CancelState.CANCELLED)
                    {
                        cancel = true;
                        p.SendBlockchange(x, y, z, oldBlock);
                    }
                }
            }
        }

        public static void OnPlayerCommand(Player p, string cmd, string args, CommandData data)
        {
            if (cmd.CaselessEq("input") && LevelHandler.HasLuaContext(p.level))
            {
                LuaContext context = LevelHandler.GetContextByLevel(p.level);
                context.CallByPlayer("onPlayerInput", p, new LuaPlayerInputEventSupplier(new PlayerInputEvent(p, args)));
                p.cancelcommand = true;
            }
        }
    }
}
