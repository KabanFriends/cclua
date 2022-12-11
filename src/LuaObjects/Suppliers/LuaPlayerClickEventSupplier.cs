using NLua;

namespace CCLua.LuaObjects.Suppliers
{
    public class LuaPlayerClickEventSupplier : LuaObjectSupplier<PlayerClickEvent>
    {
        public LuaPlayerClickEventSupplier(PlayerClickEvent e) : base(e)
        {
        }

        public static LuaTable Supply(LuaContext context, PlayerClickEvent e)
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

e.entityId = getmetatable(e).obj.entityId

e.x = getmetatable(e).obj.x

e.y = getmetatable(e).obj.y

e.z = getmetatable(e).obj.z

e.face = getmetatable(e).obj:GetFace()

return e
")[0];
        }
    }
}
