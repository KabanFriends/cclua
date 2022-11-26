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

func.x = function()
    return getmetatable(e).obj.x
end

func.y = function()
    return getmetatable(e).obj.y
end

func.z = function()
    return getmetatable(e).obj.z
end

func.block = function()
    return getmetatable(e).obj.block
end

e.cancel = function()
    return getmetatable(e).obj:Cancel()
end

return e
")[0];
        }
    }
}
