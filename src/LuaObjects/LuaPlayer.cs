using MCGalaxy;
using NLua;

namespace CCLua.LuaObjects
{
    public class LuaPlayer
    {
        public Player player;
        public PlayerData data;
        public LuaTable table;
        public bool quit;

        public LuaPlayer(Player player)
        {
            this.player = player;
            data = new PlayerData(player);
        }

        public void CreateLuaTable(LuaContext context)
        {
            context.obj[0] = this;
            context.obj[1] = Entities.CharacterHeight;

            table = (LuaTable)context.lua.DoString(@"
local p = {}
local meta = {}
local func = {}
setmetatable(p, meta)

meta.obj = context.obj[0]
meta.charHeight = context.obj[1]
meta.__tostring = function()
    return 'Player'
end

meta.__index = function(table, key)
    if func[key] ~= nil then
        return func[key]()
    end
end

-- BASIC DATA
p.name = getmetatable(p).obj.player.truename

-- PROPERTIES
func.x = function()
    if getmetatable(p).obj.quit == true then return end
    return getmetatable(p).obj.player.Pos.X / 32
end

func.y = function()
    if getmetatable(p).obj.quit == true then return end
    local m = getmetatable(p)
    return (m.obj.player.Pos.Y - m.charHeight) / 32
end

func.z = function()
    if getmetatable(p).obj.quit == true then return end
    return getmetatable(p).obj.player.Pos.Z / 32
end

func.px = function()
    if getmetatable(p).obj.quit == true then return end
    return getmetatable(p).obj.player.Pos.X
end

func.py = function()
    if getmetatable(p).obj.quit == true then return end
    local m = getmetatable(p)
    return m.obj.player.Pos.Y - m.charHeight
end

func.pz = function()
    if getmetatable(p).obj.quit == true then return end
    return getmetatable(p).obj.player.Pos.Z
end

func.yaw = function()
    if getmetatable(p).obj.quit == true then return end
    return context.caller:Call('CCLua.PlayerUtil', 'GetYaw', getmetatable(p).obj.player)
end

func.pitch = function()
    if getmetatable(p).obj.quit == true then return end
    return context.caller:Call('CCLua.PlayerUtil', 'GetPitch', getmetatable(p).obj.player)
end

func.motd = function()
    if getmetatable(p).obj.quit == true then return end
    return context.caller:Call('CCLua.PlayerUtil', 'GetTrueMotd', getmetatable(p).obj.player)
end

func.cef = function()
    if getmetatable(p).obj.quit == true then return end
    return context.caller:Call('CCLua.PlayerUtil', 'IsUsingCef', getmetatable(p).obj.player)
end

func.mobile = function()
    if getmetatable(p).obj.quit == true then return end
    return context.caller:Call('CCLua.PlayerUtil', 'IsUsingMobile', getmetatable(p).obj.player)
end

func.web = function()
    if getmetatable(p).obj.quit == true then return end
    return context.caller:Call('CCLua.PlayerUtil', 'IsUsingWeb', getmetatable(p).obj.player)
end

func.model = function()
    if getmetatable(p).obj.quit == true then return end
    return getmetatable(p).obj.player.Model
end

func.heldBlock = function()
    if getmetatable(p).obj.quit == true then return end
    return getmetatable(p).obj.player:GetHeldBlock()
end


-- FUNCTIONS
p.message = function(str)
    if getmetatable(p).obj.quit == true then return end
    context.caller:Call('CCLua.PlayerUtil', 'Message', getmetatable(p).obj.player, str)
end

p.cpeMessage = function(pos, str)
    if getmetatable(p).obj.quit == true then return end
    context.caller:Call('CCLua.PlayerUtil', 'CpeMessage', getmetatable(p).obj.player, pos, str)
end

p.kill = function(msg)
    if getmetatable(p).obj.quit == true then return end
    context.caller:Call('CCLua.PlayerUtil', 'Kill', getmetatable(p).obj.player, msg)
end

p.command = function(cmd)
    if getmetatable(p).obj.quit == true then return end
    context.caller:Call('CCLua.PlayerUtil', 'Cmd', getmetatable(p).obj.player, cmd)
end

p.tempBlock = function(block, x, y, z)
    if getmetatable(p).obj.quit == true then return end
    context.caller:Call('CCLua.PlayerUtil', 'TempBlock', getmetatable(p).obj.player, block, x, y, z)
end

p.tempChunk = function(x1, y1, z1, x2, y2, z2, x3, y3, z3, toAll)
    if getmetatable(p).obj.quit == true then return end
    if toAll == nil then
        toAll = false
    end
    context.caller:Call('CCLua.PlayerUtil', 'TempChunk', getmetatable(p).obj.player, x1, y1, z1, x2, y2, z2, x3, y3, z3, toAll)
end

p.freeze = function()
    if getmetatable(p).obj.quit == true then return end
    context.caller:Call('CCLua.PlayerUtil', 'Freeze', getmetatable(p).obj.player)
end

p.unfreeze = function()
    if getmetatable(p).obj.quit == true then return end
    context.caller:Call('CCLua.PlayerUtil', 'Unfreeze', getmetatable(p).obj.player)
end

p.look = function(x, y, z)
    if getmetatable(p).obj.quit == true then return end
    context.caller:Call('CCLua.PlayerUtil', 'Look', getmetatable(p).obj.player, x, y, z)
end

p.setEnv = function(prop, value)
    if getmetatable(p).obj.quit == true then return end
    context.caller:Call('CCLua.PlayerUtil', 'SetEnv', getmetatable(p).obj.player, prop, value)
end

p.setMotd = function(motd)
    if getmetatable(p).obj.quit == true then return end
    context.caller:Call('CCLua.PlayerUtil', 'SetMotd', getmetatable(p).obj.player, motd)
end

p.setSpawn = function(x, y, z)
    if getmetatable(p).obj.quit == true then return end
    context.caller:Call('CCLua.PlayerUtil', 'SetSpawn', getmetatable(p).obj.player, x, y, z)
end

p.addHotkey = function(input, key, ...)
    if getmetatable(p).obj.quit == true then return end
    if {...} == nil then
        context.caller:Call('CCLua.PlayerUtil', 'AddHotkey', getmetatable(p).obj.player, input, key)
    else
        context.caller:Call('CCLua.PlayerUtil', 'AddHotkey', getmetatable(p).obj.player, input, key, {...})
    end
end

p.removeHotkey = function(key, ...)
    if getmetatable(p).obj.quit == true then return end
    if {...} == nil then
        context.caller:Call('CCLua.PlayerUtil', 'RemoveHotkey', getmetatable(p).obj.player, key)
    else
        context.caller:Call('CCLua.PlayerUtil', 'RemoveHotkey', getmetatable(p).obj.player, key, {...})
    end
end

p.setReach = function(reach)
    if getmetatable(p).obj.quit == true then return end
    context.caller:Call('CCLua.PlayerUtil', 'SetReach', getmetatable(p).obj.player, reach)
end

p.setModel = function(model)
    if getmetatable(p).obj.quit == true then return end
    context.caller:Call('CCLua.PlayerUtil', 'SetModel', getmetatable(p).obj.player, model)
end

p.launch = function(x, y, z, xadd, yadd, zadd)
    if getmetatable(p).obj.quit == true then return end
    context.caller:Call('CCLua.PlayerUtil', 'Launch', getmetatable(p).obj.player, x, y, z, xadd, yadd, zadd)
end

p.setHotbar = function(slot, block)
    if getmetatable(p).obj.quit == true then return end
    context.caller:Call('CCLua.PlayerUtil', 'SetHotbar', getmetatable(p).obj.player, slot, block)
end

p.teleport = function(x, y, z, yaw, pitch)
    if getmetatable(p).obj.quit == true then return end
    if yaw == nil or pitch == nil then
        context.caller:Call('CCLua.PlayerUtil', 'Teleport', getmetatable(p).obj.player, x, y, z)
    else
        context.caller:Call('CCLua.PlayerUtil', 'TeleportRot', getmetatable(p).obj.player, x, y, z, yaw, pitch)
    end
end

p.playParticle = function(particleName, x, y, z, originX, originY, originZ)
    if getmetatable(p).obj.quit == true then return end
    if originX == nil or originY == nil or originZ == nil then
        context.caller:Call('CCLua.PlayerUtil', 'PlayParticle', getmetatable(p).obj.player, particleName, x, y, z, x, y, z)
    else
        context.caller:Call('CCLua.PlayerUtil', 'PlayParticle', getmetatable(p).obj.player, particleName, x, y, z, originX, originY, originZ)
    end
end

return p
")[0];
        }
    }
}
