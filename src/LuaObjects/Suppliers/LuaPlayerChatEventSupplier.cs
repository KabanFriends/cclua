using NLua;

namespace CCLua.LuaObjects.Suppliers
{
    public class LuaPlayerChatEventSupplier : LuaObjectSupplier<PlayerChatEvent>
    {
        public LuaPlayerChatEventSupplier(PlayerChatEvent e) : base(e)
        {
        }

        public static LuaTable Supply(LuaContext context, PlayerChatEvent e)
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

e.message = getmetatable(e).obj.message

e.cancel = function()
    return getmetatable(e).obj:Cancel()
end

return e
")[0];
        }
    }
}
