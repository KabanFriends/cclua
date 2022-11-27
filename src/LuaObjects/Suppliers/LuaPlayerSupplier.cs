using MCGalaxy;
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
    return getmetatable(p).obj.Pos.X / 32
end

func.y = function()
    return (getmetatable(p).obj.Pos.Y - charHeight) / 32
end

func.z = function()
    return getmetatable(p).obj.Pos.Z / 32
end

func.px = function()
    return getmetatable(p).obj.Pos.X
end

func.py = function()
    return getmetatable(p).obj.Pos.Y - charHeight
end

func.pz = function()
    return getmetatable(p).obj.Pos.Z
end

func.yaw = function()
    return context.caller:Call('CCLua.PlayerUtil', 'GetYaw', getmetatable(p).obj)
end

func.pitch = function()
    return context.caller:Call('CCLua.PlayerUtil', 'GetPitch', getmetatable(p).obj)
end

func.motd = function()
    return context.caller:Call('CCLua.PlayerUtil', 'GetTrueMotd', getmetatable(p).obj)
end

func.cef = function()
    return context.caller:Call('CCLua.PlayerUtil', 'IsUsingCef', getmetatable(p).obj)
end

func.mobile = function()
    return context.caller:Call('CCLua.PlayerUtil', 'IsUsingMobile', getmetatable(p).obj)
end

func.web = function()
    return context.caller:Call('CCLua.PlayerUtil', 'IsUsingWeb', getmetatable(p).obj)
end

func.model = function()
    return getmetatable(p).obj.Model
end

func.heldBlock = function()
    return getmetatable(p).obj:GetHeldBlock()
end


-- FUNCTIONS
p.message = function(str)
    getmetatable(p).obj:Message(tostring(str))
end

p.cpeMessage = function(pos, str)
    context.caller:Call('CCLua.PlayerUtil', 'CpeMessage', getmetatable(p).obj, pos, str)
end

p.kill = function(msg)
    context.caller:Call('CCLua.PlayerUtil', 'Kill', getmetatable(p).obj, msg)
end

p.command = function(cmd)
    context.caller:Call('CCLua.PlayerUtil', 'Cmd', getmetatable(p).obj, cmd)
end

p.tempBlock = function(block, x, y, z)
    context.caller:Call('CCLua.PlayerUtil', 'TempBlock', getmetatable(p).obj, block, x, y, z)
end

p.tempChunk = function(x1, y1, z1, x2, y2, z2, x3, y3, z3, toAll)
    if toAll == nil then
        toAll = false
    end
    context.caller:Call('CCLua.PlayerUtil', 'TempChunk', getmetatable(p).obj, x1, y1, z1, x2, y2, z2, x3, y3, z3, toAll)
end

p.freeze = function()
    context.caller:Call('CCLua.PlayerUtil', 'Freeze', getmetatable(p).obj)
end

p.unfreeze = function()
    context.caller:Call('CCLua.PlayerUtil', 'Unfreeze', getmetatable(p).obj)
end

p.look = function(x, y, z)
    context.caller:Call('CCLua.PlayerUtil', 'Look', getmetatable(p).obj, x, y, z)
end

p.setEnv = function(prop, value)
    context.caller:Call('CCLua.PlayerUtil', 'SetEnv', getmetatable(p).obj, prop, value)
end

p.setMotd = function(motd)
    context.caller:Call('CCLua.PlayerUtil', 'SetMotd', getmetatable(p).obj, motd)
end

p.setSpawn = function(x, y, z)
    context.caller:Call('CCLua.PlayerUtil', 'SetSpawn', getmetatable(p).obj, x, y, z)
end

p.addHotkey = function(input, key, ...)
    if {...} == nil then
        context.caller:Call('CCLua.PlayerUtil', 'AddHotkey', getmetatable(p).obj, input, key)
    else
        context.caller:Call('CCLua.PlayerUtil', 'AddHotkey', getmetatable(p).obj, input, key, {...})
    end
end

p.removeHotkey = function(key, ...)
    if {...} == nil then
        context.caller:Call('CCLua.PlayerUtil', 'RemoveHotkey', getmetatable(p).obj, key)
    else
        context.caller:Call('CCLua.PlayerUtil', 'RemoveHotkey', getmetatable(p).obj, key, {...})
    end
end

p.setReach = function(reach)
    context.caller:Call('CCLua.PlayerUtil', 'SetReach', getmetatable(p).obj, reach)
end

p.setModel = function(model)
    context.caller:Call('CCLua.PlayerUtil', 'SetModel', getmetatable(p).obj, model)
end

p.launch = function(x, y, z, xadd, yadd, zadd)
    context.caller:Call('CCLua.PlayerUtil', 'Launch', getmetatable(p).obj, x, y, z, xadd, yadd, zadd)
end

p.setHotbar = function(slot, block)
    context.caller:Call('CCLua.PlayerUtil', 'SetHotbar', getmetatable(p).obj, slot, block)
end

p.teleport = function(x, y, z, yaw, pitch)
    if yaw == nil or pitch == nil then
        context.caller:Call('CCLua.PlayerUtil', 'Teleport', getmetatable(p).obj, x, y, z)
    else
        context.caller:Call('CCLua.PlayerUtil', 'TeleportRot', getmetatable(p).obj, x, y, z, yaw, pitch)
    end
end

return p
")[0];
        }
    }
}
