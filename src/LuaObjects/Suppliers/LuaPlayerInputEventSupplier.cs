using NLua;

namespace CCLua.LuaObjects.Suppliers
{
    public class LuaPlayerInputEventSupplier : LuaObjectSupplier<PlayerInputEvent>
    {
        public LuaPlayerInputEventSupplier(PlayerInputEvent e) : base(e)
        {
        }

        public static LuaTable Supply(Lua lua, PlayerInputEvent e)
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

func.message = function()
    return getmetatable(e).obj.message
end

func.args = function()
    local obj = getmetatable(e).obj
    args = {}
    for i = 1, obj.args.Length do
        args[i] = obj.args[i - 1]
    end
    return args
end

return e
")[0];
        }
    }
}
