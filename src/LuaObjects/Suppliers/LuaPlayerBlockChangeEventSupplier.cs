using NLua;

namespace CCLua.LuaObjects.Suppliers
{
    public class LuaPlayerBlockChangeEventSupplier : LuaObjectSupplier<PlayerBlockChangeEvent>
    {
        public LuaPlayerBlockChangeEventSupplier(PlayerBlockChangeEvent e) : base(e)
        {
        }

        public static LuaTable Supply(LuaContext context, PlayerBlockChangeEvent e)
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

e.x = getmetatable(e).obj.x

e.y = getmetatable(e).obj.y

e.z = getmetatable(e).obj.z

e.block = getmetatable(e).obj.block

e.cancel = function()
    return getmetatable(e).obj:Cancel()
end

return e
")[0];
        }
    }
}
