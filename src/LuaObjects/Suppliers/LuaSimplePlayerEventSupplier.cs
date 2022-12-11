using NLua;

namespace CCLua.LuaObjects.Suppliers
{
    public class LuaSimplePlayerEventSupplier : LuaObjectSupplier<SimplePlayerEvent>
    {
        public LuaSimplePlayerEventSupplier(SimplePlayerEvent e) : base(e)
        {
        }

        public static LuaTable Supply(LuaContext context, SimplePlayerEvent e)
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
