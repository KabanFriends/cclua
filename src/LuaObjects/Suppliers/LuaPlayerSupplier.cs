﻿using MCGalaxy;
using NLua;

namespace CCLua.LuaObjects.Suppliers
{
    public class LuaPlayerSupplier : LuaObjectSupplier<Player>
    {
        public LuaPlayerSupplier(Player player) : base(player)
        {
        }

        public static LuaTable Supply(Lua lua, Player p)
        {
            lua["playerName"] = p.truename;
            lua["charHeight"] = Entities.CharacterHeight;

            return (LuaTable)lua.DoString(@"
local p = context:GetLuaPlayer(playerName)
local meta = getmetatable(p)
local func = {}

-- BASIC DATA
p.name = getmetatable(p).obj.truename

-- METATABLE INDEXES
meta.__index = function(table, key)
    if func[key] ~= nil then
        return func[key]()
    end
end

func.x = function()
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    return getmetatable(p).obj.Pos.X / 32
end

func.y = function()
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    return (getmetatable(p).obj.Pos.Y - charHeight) / 32
end

func.z = function()
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    return getmetatable(p).obj.Pos.Z / 32
end

func.px = function()
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    return getmetatable(p).obj.Pos.X
end

func.py = function()
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    return getmetatable(p).obj.Pos.Y - charHeight
end

func.pz = function()
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    return getmetatable(p).obj.Pos.Z
end

func.yaw = function()
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    return context.caller:Call('CCLua.PlayerUtil', 'GetYaw', getmetatable(p).obj)
end

func.pitch = function()
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    return context.caller:Call('CCLua.PlayerUtil', 'GetPitch', getmetatable(p).obj)
end

func.motd = function()
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    return context.caller:Call('CCLua.PlayerUtil', 'GetTrueMotd', getmetatable(p).obj)
end

func.cef = function()
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    return context.caller:Call('CCLua.PlayerUtil', 'IsUsingCef', getmetatable(p).obj)
end

func.mobile = function()
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    return context.caller:Call('CCLua.PlayerUtil', 'IsUsingMobile', getmetatable(p).obj)
end

func.web = function()
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    return context.caller:Call('CCLua.PlayerUtil', 'IsUsingWeb', getmetatable(p).obj)
end

func.model = function()
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    return getmetatable(p).obj.Model
end

func.heldBlock = function()
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    return getmetatable(p).obj:GetHeldBlock()
end


-- FUNCTIONS
p.message = function(str)
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    context.caller:Call('CCLua.PlayerUtil', 'Message', getmetatable(p).obj, str)
end

p.cpeMessage = function(pos, str)
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    context.caller:Call('CCLua.PlayerUtil', 'CpeMessage', getmetatable(p).obj, pos, str)
end

p.kill = function(msg)
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    context.caller:Call('CCLua.PlayerUtil', 'Kill', getmetatable(p).obj, msg)
end

p.command = function(cmd)
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    context.caller:Call('CCLua.PlayerUtil', 'Cmd', getmetatable(p).obj, cmd)
end

p.tempBlock = function(block, x, y, z)
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    context.caller:Call('CCLua.PlayerUtil', 'TempBlock', getmetatable(p).obj, block, x, y, z)
end

p.tempChunk = function(x1, y1, z1, x2, y2, z2, x3, y3, z3, toAll)
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    if toAll == nil then
        toAll = false
    end
    context.caller:Call('CCLua.PlayerUtil', 'TempChunk', getmetatable(p).obj, x1, y1, z1, x2, y2, z2, x3, y3, z3, toAll)
end

p.freeze = function()
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    context.caller:Call('CCLua.PlayerUtil', 'Freeze', getmetatable(p).obj)
end

p.unfreeze = function()
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    context.caller:Call('CCLua.PlayerUtil', 'Unfreeze', getmetatable(p).obj)
end

p.look = function(x, y, z)
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    context.caller:Call('CCLua.PlayerUtil', 'Look', getmetatable(p).obj, x, y, z)
end

p.setEnv = function(prop, value)
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    context.caller:Call('CCLua.PlayerUtil', 'SetEnv', getmetatable(p).obj, prop, value)
end

p.setMotd = function(motd)
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    context.caller:Call('CCLua.PlayerUtil', 'SetMotd', getmetatable(p).obj, motd)
end

p.setSpawn = function(x, y, z)
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    context.caller:Call('CCLua.PlayerUtil', 'SetSpawn', getmetatable(p).obj, x, y, z)
end

p.addHotkey = function(input, key, ...)
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    if {...} == nil then
        context.caller:Call('CCLua.PlayerUtil', 'AddHotkey', getmetatable(p).obj, input, key)
    else
        context.caller:Call('CCLua.PlayerUtil', 'AddHotkey', getmetatable(p).obj, input, key, {...})
    end
end

p.removeHotkey = function(key, ...)
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    if {...} == nil then
        context.caller:Call('CCLua.PlayerUtil', 'RemoveHotkey', getmetatable(p).obj, key)
    else
        context.caller:Call('CCLua.PlayerUtil', 'RemoveHotkey', getmetatable(p).obj, key, {...})
    end
end

p.setReach = function(reach)
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    context.caller:Call('CCLua.PlayerUtil', 'SetReach', getmetatable(p).obj, reach)
end

p.setModel = function(model)
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    context.caller:Call('CCLua.PlayerUtil', 'SetModel', getmetatable(p).obj, model)
end

p.launch = function(x, y, z, xadd, yadd, zadd)
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    context.caller:Call('CCLua.PlayerUtil', 'Launch', getmetatable(p).obj, x, y, z, xadd, yadd, zadd)
end

p.setHotbar = function(slot, block)
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    context.caller:Call('CCLua.PlayerUtil', 'SetHotbar', getmetatable(p).obj, slot, block)
end

p.teleport = function(x, y, z, yaw, pitch)
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    if yaw == nil or pitch == nil then
        context.caller:Call('CCLua.PlayerUtil', 'Teleport', getmetatable(p).obj, x, y, z)
    else
        context.caller:Call('CCLua.PlayerUtil', 'TeleportRot', getmetatable(p).obj, x, y, z, yaw, pitch)
    end
end

p.playParticle = function(particleName, x, y, z, originX, originY, originZ)
    if context:IsPlayerInLevel(getmetatable(p).obj) == false then return end
    if originX == nil or originY == nil or originZ == nil then
        context.caller:Call('CCLua.PlayerUtil', 'PlayParticle', getmetatable(p).obj, particleName, x, y, z, x, y, z)
    else
        context.caller:Call('CCLua.PlayerUtil', 'PlayParticle', getmetatable(p).obj, particleName, x, y, z, originX, originY, originZ)
    end
end

return p
")[0];
        }
    }
}
