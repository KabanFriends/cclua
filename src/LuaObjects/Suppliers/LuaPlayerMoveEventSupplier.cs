using NLua;

namespace CCLua.LuaObjects.Suppliers
{
    public class LuaPlayerMoveEventSupplier : LuaObjectSupplier<PlayerMoveEvent>
    {
        public LuaPlayerMoveEventSupplier(PlayerMoveEvent e) : base(e)
        {
        }

        public static LuaTable Supply(LuaContext context, PlayerMoveEvent e)
        {
            context.obj[0] = e;

            return (LuaTable)context.lua.DoString(@"
local e = {}
local meta = {}
setmetatable(e, meta)

meta.obj = context.obj[0]
meta.__tostring = function()
    return 'Event'
end

e.player = context.caller:Call('CCLua.LuaObjects.Suppliers.LuaPlayerSupplier', 'Supply', context, getmetatable(e).obj.player)

e.yaw = getmetatable(e).obj.yaw

e.pitch = getmetatable(e).obj.pitch

e.x = getmetatable(e).obj.x

e.y = getmetatable(e).obj.y

e.z = getmetatable(e).obj.z

return e
")[0];
        }
    }
}
