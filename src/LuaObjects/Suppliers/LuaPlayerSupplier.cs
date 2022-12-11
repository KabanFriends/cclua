using MCGalaxy;
using NLua;

namespace CCLua.LuaObjects.Suppliers
{
    public class LuaPlayerSupplier : LuaObjectSupplier<Player>
    {
        public LuaPlayerSupplier(Player player) : base(player)
        {
        }

        public static LuaTable Supply(LuaContext context, Player p)
        {
            context.obj[0] = p.truename;
            return (LuaTable)context.lua.DoString("return context:GetLuaPlayer(context.obj[0]).table")[0];
        }
    }
}
