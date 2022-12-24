using CCLua.Commands;
using CCLua.PluginEvents;
using MCGalaxy;
using MCGalaxy.Events.LevelEvents;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Util;
using System.Collections.Generic;
using System.IO;

namespace CCLua
{
    public class CCLuaPlugin : Plugin
    {
        public override string name => Constants.PLUGIN_NAME;

        public override string creator => Constants.PLUGIN_CREATOR;

        public override string MCGalaxy_Version => Constants.MCGALAXY_VERSION;

        private CmdLua luaCommand;

        private CmdStaffLua staffLuaCommand;

        private CmdTempBlock tempBlockCommand;

        private CmdTempChunk tempChunkCommand;

        private CmdInput inputCommand;

        public static Command tempBlockToUse;

        public static Command tempChunkToUse;

        public static TextFile staffMapsFile;

        public static HashSet<string> staffMaps;

        public static Dictionary<string, string> usernameMap;

        public override void Load(bool auto)
        {
            Command tempblock = Command.Find("tempblock");
            if (tempblock != null)
            {
                tempBlockToUse = tempblock;
            } else
            {
                tempBlockCommand = new CmdTempBlock();
                Command.Register(tempBlockCommand);
                tempBlockToUse = tempBlockCommand;
            }

            Command tempchunk = Command.Find("tempchunk");
            if (tempchunk != null)
            {
                tempChunkToUse = tempchunk;
            } else
            {
                tempChunkCommand = new CmdTempChunk();
                Command.Register(tempChunkCommand);
                tempChunkToUse = tempChunkCommand;
            }

            inputCommand = new CmdInput();
            Command.Register(inputCommand);

            luaCommand = new CmdLua();
            Command.Register(luaCommand);

            staffLuaCommand = new CmdStaffLua();
            Command.Register(staffLuaCommand);

            OnLevelLoadedEvent.Register(PluginLevelEvents.OnLevelLoaded, Priority.High);
            OnLevelUnloadEvent.Register(PluginLevelEvents.OnLevelUnload, Priority.High);

            OnJoinedLevelEvent.Register(PluginPlayerEvents.OnJoinedLevel, Priority.High);
            OnJoiningLevelEvent.Register(PluginPlayerEvents.OnJoiningLevel, Priority.Low);
            OnPlayerFinishConnectingEvent.Register(PluginPlayerEvents.OnPlayerFinishConnecting, Priority.Low);
            OnPlayerDisconnectEvent.Register(PluginPlayerEvents.OnPlayerDisconnect, Priority.Low);
            OnPlayerMoveEvent.Register(PluginPlayerEvents.OnPlayerMove, Priority.Low);
            OnPlayerClickEvent.Register(PluginPlayerEvents.OnPlayerClick, Priority.Low);
            OnPlayerChatEvent.Register(PluginPlayerEvents.OnPlayerChat, Priority.Low);
            OnBlockChangingEvent.Register(PluginPlayerEvents.OnBlockChanging, Priority.Low);
            OnPlayerCommandEvent.Register(PluginPlayerEvents.OnPlayerCommand, Priority.Low);

            Directory.CreateDirectory(Constants.CCLUA_BASE_DIR);
            Directory.CreateDirectory(Constants.CCLUA_BASE_DIR + Constants.TEMP_DIR);
            Directory.CreateDirectory(Constants.CCLUA_BASE_DIR + Constants.SCRIPT_DIR);
            Directory.CreateDirectory(Constants.CCLUA_BASE_DIR + Constants.STORAGE_DIR);

            staffMapsFile = new TextFile(Constants.CCLUA_BASE_DIR + Constants.STAFFMAPS_FILE, null);
            staffMapsFile.EnsureExists();
            staffMaps = new HashSet<string>();

            usernameMap = new Dictionary<string, string>();

            foreach (string line in staffMapsFile.GetText())
            {
                if (LevelInfo.MapExists(line))
                {
                    staffMaps.Add(line);
                }
            }

            LevelHandler.TryCreateLuaContext(Server.mainLevel); //plugins are loaded after main is loaded
        }

        public override void Unload(bool auto)
        {
            foreach(KeyValuePair<string, LuaContext> pair in LevelHandler.contexts)
            {
                pair.Value.Call("onLevelStop");
                pair.Value.Stop();
            }

            if (LevelHandler.HasLuaContext(Server.mainLevel))
            {
                LevelHandler.StopLuaContext(Server.mainLevel);
            }

            if (tempBlockCommand != null)
            {
                Command.Unregister(tempBlockCommand);
            }
            
            if (tempChunkCommand != null)
            {
                Command.Unregister(tempChunkCommand);
            }

            Command.Unregister(luaCommand);
            Command.Unregister(inputCommand);
            Command.Unregister(staffLuaCommand);

            OnLevelLoadedEvent.Unregister(PluginLevelEvents.OnLevelLoaded);
            OnLevelUnloadEvent.Unregister(PluginLevelEvents.OnLevelUnload);

            OnJoinedLevelEvent.Unregister(PluginPlayerEvents.OnJoinedLevel);
            OnJoiningLevelEvent.Unregister(PluginPlayerEvents.OnJoiningLevel);
            OnPlayerFinishConnectingEvent.Unregister(PluginPlayerEvents.OnPlayerFinishConnecting);
            OnPlayerDisconnectEvent.Unregister(PluginPlayerEvents.OnPlayerDisconnect);
            OnPlayerMoveEvent.Unregister(PluginPlayerEvents.OnPlayerMove);
            OnPlayerClickEvent.Unregister(PluginPlayerEvents.OnPlayerClick);
            OnPlayerChatEvent.Unregister(PluginPlayerEvents.OnPlayerChat);
            OnBlockChangingEvent.Unregister(PluginPlayerEvents.OnBlockChanging);
            OnPlayerCommandEvent.Unregister(PluginPlayerEvents.OnPlayerCommand);
        }
    }
}
