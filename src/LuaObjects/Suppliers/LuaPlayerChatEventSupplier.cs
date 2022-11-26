using NLua;

namespace CCLua.LuaObjects.Suppliers
{
    public class LuaPlayerChatEventSupplier : LuaObjectSupplier<PlayerChatEvent>
    {
        public LuaPlayerChatEventSupplier(PlayerChatEvent e) : base(e)
        {
        }

        public static LuaTable Supply(Lua lua, PlayerChatEvent e)
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

e.message = getmetatable(e).obj.message

e.cancel = function()
    return getmetatable(e).obj:Cancel()
end

return e
")[0];
        }
    }
}
