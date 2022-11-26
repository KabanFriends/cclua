using MCGalaxy;
using MCGalaxy.Commands;
using MCGalaxy.Maths;
using NLua;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
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
            BlockID id;
            if (block is double dbl)
            {
                id = Convert.ToUInt16(dbl);
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
    }
}
