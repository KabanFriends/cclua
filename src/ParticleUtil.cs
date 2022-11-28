using MCGalaxy;
using MCGalaxy.Network;
using NLua;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CCLua
{
    public static class ParticleUtil
    {
        public static LuaTable GetParticleManager(Lua lua)
        {
            return (LuaTable)lua.DoString(@"
local table = {}

table.new = function()
    local p = {}
    local meta = {}
    setmetatable(p, meta)

    meta.__tostring = function()
        return 'Particle'
    end

    p.x = 0
    p.y = 0
    p.width = 10
    p.height = 10
    
    p.tintRed = 255
    p.tintGreen = 255
    p.tintBlue = 255

    p.fullBright = true

    p.frameCount = 1
    p.particleCount = 1

    p.size = 8
    p.sizeVariation = 0

    p.spread = 0
    p.speed = 0
    p.gravity = 0

    p.lifetime = 1
    p.lifetimeVariation = 0

    p.expireUponTouchingGround = true
    p.collideSolid = true
    p.collideLiquid = true
    p.collideLeaves = true

    return p
end

table.register = function(name, par)
    context.caller:Call('CCLua.ParticleUtil', 'RegisterParticle', context, name, par)
end

table.unregister = function(name)
    context.caller:Call('CCLua.ParticleUtil', 'UnregisterParticle', context, name)
end

return table
")[0];
        }

        public static void RegisterParticle(LuaContext context, string name, LuaTable particle)
        {
            bool doRegister = false;
            bool editExisting = false;
            byte id;

            if (context.particleIds.Count > 0)
            {
                if (context.particleIds.ContainsKey(name))
                {
                    editExisting = true;
                    id = context.particleIds[name];
                    doRegister = true;
                } else
                {
                    var idDict = context.particleIds.ToDictionary(x => x.Value, x => x.Key);

                    for (id = 0; id < idDict.Count; id++)
                    {
                        if (!idDict.ContainsKey(id))
                        {
                            doRegister = true;
                            break;
                        }
                    }
                }
            } else
            {
                id = 0;
                doRegister = true;
            }

            if (!doRegister)
            {
                return;
            }

            if (editExisting)
            {
                context.particleData[name] = particle;
                context.particleIds[name] = id;
            } else
            {
                context.particleData.Add(name, particle);
                context.particleIds.Add(name, id);
            }

            foreach (Player p in context.level.players)
            {
                context.SendParticle(p, name);
            }
        }

        public static void UnregisterParticle(LuaContext context, string name)
        {
            if (!context.particleData.ContainsKey(name) || !context.particleIds.ContainsKey(name)) return;

            context.particleData.Remove(name);
            context.particleIds.Remove(name);
        }
    }
}
