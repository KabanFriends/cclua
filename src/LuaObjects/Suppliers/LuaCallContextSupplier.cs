using NLua;

namespace CCLua.LuaObjects.Suppliers
{
    public class LuaCallContextSupplier : LuaObjectSupplier<CallContext>
    {
        public LuaCallContextSupplier(CallContext e) : base(e)
        {
        }

        public static LuaTable Supply(LuaContext context, CallContext e)
        {
            context.obj[0] = e;

            return (LuaTable)context.lua.DoString(@"
local e = {}
local meta = {}
local func = {}
setmetatable(e, meta)

meta.obj = context.obj[0]
meta.__tostring = function()
    return 'CallContext'
end

meta.__index = function(table, key)
    if func[key] ~= nil then
        return func[key]()
    end
end

func.player = function()
    return context.caller:Call('CCLua.LuaObjects.Suppliers.LuaPlayerSupplier', 'Supply', context, getmetatable(e).obj.player)
end

func.mbX = function()
    return getmetatable(e).obj.mbX
end

func.mbY = function()
    return getmetatable(e).obj.mbY
end

func.mbZ = function()
    return getmetatable(e).obj.mbZ
end

return e
")[0];
        }
    }
}
