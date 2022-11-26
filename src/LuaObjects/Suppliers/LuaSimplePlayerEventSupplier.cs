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
local func = {}
setmetatable(e, meta)

meta.obj = csEvent
meta.__tostring = function()
    return 'Event'
end

meta.__index = function(table, key)
    if func[key] ~= nil then
        return func[key]()
    end
end

func.player = function()
    return context.caller:Call('CCLua.LuaObjects.Suppliers.LuaPlayerSupplier', 'Supply', context.lua, getmetatable(e).obj.player)
end

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
