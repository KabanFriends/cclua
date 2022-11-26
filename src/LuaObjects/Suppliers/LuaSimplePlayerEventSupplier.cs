using NLua;

namespace CCLua.LuaObjects.Suppliers
{
    public class LuaSimplePlayerEventSupplier : LuaObjectSupplier<SimplePlayerEvent>
    {
        public LuaSimplePlayerEventSupplier(SimplePlayerEvent e) : base(e)
        {
        }

        public static LuaTable Supply(Lua lua, SimplePlayerEvent e)
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

e.cancel = function()
    getmetatable(e).obj:Cancel()
end

e.uncancel = function()
    getmetatable(e).obj:Uncancel()
end

return e
")[0];
        }
    }
}
