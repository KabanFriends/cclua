using NLua;

namespace CCLua.LuaObjects.Suppliers
{
    public class LuaPlayerBlockChangeEventSupplier : LuaObjectSupplier<PlayerBlockChangeEvent>
    {
        public LuaPlayerBlockChangeEventSupplier(PlayerBlockChangeEvent e) : base(e)
        {
        }

        public static LuaTable Supply(Lua lua, PlayerBlockChangeEvent e)
        {
            lua["csEvent"] = e;

            return (LuaTable)lua.DoString(@"
local e = {}
local meta = {}
setmetatable(e, meta)

meta.obj = csEvent
meta.__tostring = function()
    return 'Event'
end

e.player = context.caller:Call('CCLua.LuaObjects.Suppliers.LuaPlayerSupplier', 'Supply', context.lua, getmetatable(e).obj.player)

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
