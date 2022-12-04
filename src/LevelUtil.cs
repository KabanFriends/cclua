using MCGalaxy;
using MCGalaxy.Commands;
using MCGalaxy.Maths;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLua;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using BlockID = System.UInt16;

namespace CCLua
{
    public static class LevelUtil
    {
        public static string[] blockCoreNames = (string[])typeof(Block).GetField("coreNames", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

        public static LuaTable GetLevelObject(Lua lua)
        {
            return (LuaTable)lua.DoString(@"
local l = {}
local meta = {}
local func = {}
setmetatable(l, meta)

meta.obj = csEvent
meta.__tostring = function()
    return 'Level'
end

meta.__index = function(table, key)
    if func[key] ~= nil then
        return func[key]()
    end
end

func.playerCount = function()
    return context.luaPlayers.Count
end

func.epochMs = function()
    return context:GetCurrentTimeMillis()
end

l.name = context.level.name

l.broadcast = function(text)
    context.caller:Call('CCLua.LevelUtil', 'Broadcast', context.level, tostring(text))
end

l.getPlayers = function()
    local list = {}
    context.caller:Call('CCLua.LevelUtil', 'SetPlayersTable', context, list)
    return list
end

l.getPlayer = function(name)
    return context.caller:Call('CCLua.LevelUtil', 'GetPlayer', context, name)
end

l.getBlockName = function(id)
    return context.caller:Call('CCLua.LevelUtil', 'GetBlockName', context.level, id)
end

l.getBlockId = function(name)
    return context.caller:Call('CCLua.LevelUtil', 'GetBlockId', context.level, name)
end

l.placeBlock = function(block, x, y, z)
    context.caller:Call('CCLua.LevelUtil', 'PlaceBlock', context.level, block, x, y, z)
end

l.getBlockAt = function(x, y, z)
    return context.caller:Call('CCLua.LevelUtil', 'GetBlockAt', context.level, x, y, z)
end

l.writeData = function(key, data)
    context.caller:Call('CCLua.LevelUtil', 'WriteData', context, key, data)
end

l.readData = function(key)
    return context.caller:Call('CCLua.LevelUtil', 'ReadData', context, key)
end

l.getAllData = function()
    local list = {}
    context.caller:Call('CCLua.LevelUtil', 'SetAllDataTable', context, list)
    return list
end

l.dataExists = function(key)
    if type(key) ~= 'string' then
        return false
    end
    return context.dataJson:ContainsKey(key)
end

l.removeData = function(key)
    if type(key) ~= 'string' then
        return
    end
    context.dataJson:Remove(key)
end

l.clearAllData = function()
    context.dataJson:RemoveAll()
end

return l
")[0];
        }

        public static bool IsOsLevel(Level level)
        {
            if (CCLuaPlugin.staffMaps.Contains(level.name))
            {
                return false;
            }
            return true;
        }

        public static void SetPlayersTable(LuaContext context, LuaTable table)
        {
            int i = 1;
            foreach (LuaTable player in context.luaPlayers.Values)
            {
                table[i] = player;
                i++;
            }
        }

        public static LuaTable GetPlayer(LuaContext context, string name)
        {
            return context.luaPlayers.ContainsKey(name) ? context.luaPlayers[name] : null;
        }

        public static string GetBlockName(Level level, double block)
        {
            ushort id = Convert.ToUInt16(block);
            BlockDefinition def = level.GetBlockDef(id);
            if (def != null)
            {
                return def.Name;
            }

            if (id >= 256)
            {
                return Block.ToRaw(id).ToString();
            }

            return blockCoreNames[id];
        }

        public static BlockID? GetBlockId(Level level, string input)
        {
            BlockDefinition[] array = level.CustomBlockDefs;
            if (ushort.TryParse(input, out BlockID rawResult) && (rawResult < 66 || (rawResult <= 255 && array[Block.FromRaw(rawResult)] != null)))
            {
                return Block.FromRaw(rawResult);
            }

            BlockID? result = null;
            for (int i = 1; i < array.Length; i++)
            {
                BlockDefinition blockDefinition = array[i];
                if (blockDefinition != null && blockDefinition.Name.Replace(" ", "").CaselessEq(input))
                {
                    result = blockDefinition.GetBlock();
                }
            }

            if (result != null)
            {
                return result;
            }

            if (!Block.Aliases.TryGetValue(input.ToLower(), out var value))
            {
                return null;
            }

            return value;
        }

        public static void PlaceBlock(Level level, object block, double x, double y, double z)
        {
            BlockID id = 0;
            if (IsLuaNumber(block))
            {
                if (block is int intId) id = Convert.ToUInt16(intId);
                else if (block is long longId) id = Convert.ToUInt16(longId);
                else if (block is double doubleId) id = Convert.ToUInt16(doubleId);
                else if (block is float floatId) id = Convert.ToUInt16(floatId);

                id = Block.FromRaw(id);
            } else if (block is string str)
            {
                var tempId = GetBlockId(level, str);
                if (tempId == null)
                {
                    return;
                }
                id = (BlockID)tempId;
            } else
            {
                return;
            }

            Vec3S32 coords = new Vec3S32((int)x, (int)y, (int)z);

            coords = level.ClampPos(coords);
            level.SetBlock((ushort)coords.X, (ushort)coords.Y, (ushort)coords.Z, id);
            level.BroadcastChange((ushort)coords.X, (ushort)coords.Y, (ushort)coords.Z, id);
        }

        public static BlockID GetBlockAt(Level level, double x, double y, double z)
        {
            Vec3S32 coords = new Vec3S32((int)x, (int)y, (int)z);
            return Block.ToRaw(Block.Convert(level.GetBlock((ushort)coords.X, (ushort)coords.Y, (ushort)coords.Z)));
        }

        public static void WriteData(LuaContext context, string key, object data)
        {
            if (data is string || IsLuaNumber(data) || data is bool)
            {
                if (data is string strData) context.dataJson[key] = strData;
                else if (data is int intData) context.dataJson[key] = intData;
                else if (data is long longData) context.dataJson[key] = longData;
                else if (data is double doubleData) context.dataJson[key] = doubleData;
                else if (data is float floatData) context.dataJson[key] = floatData;
                else if (data is bool boolData) context.dataJson[key] = boolData;
            } else
            {
                throw new UserScriptException("Data to write must be a string, a double or a boolean!");
            }
        }

        public static object ReadData(LuaContext context, string key)
        {
            if (!context.dataJson.ContainsKey(key))
            {
                return null;
            }

            return GetLuaObject(context.dataJson[key]);
        }

        public static void SetAllDataTable(LuaContext context, LuaTable table)
        {
            foreach (var entry in context.dataJson)
            {
                table[entry.Key] = GetLuaObject(entry.Value);
            }
        }

        private static bool IsLuaNumber(object obj)
        {
            return obj is int || obj is long || obj is float || obj is double;
        }

        private static object GetLuaObject(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.String:
                    return (string)token;
                case JTokenType.Integer:
                    return Convert.ToInt32(token);
                case JTokenType.Float:
                    return Convert.ToSingle(token);
                case JTokenType.Boolean:
                    return (bool)token;
            }
            return null;
        }

        public static void Broadcast(Level level, string message)
        {
            foreach (Player p in level.players)
            {
                PlayerUtil.Message(p, message);
            }
        }
    }
}
