using MCGalaxy;

namespace CCLua.LuaObjects
{
    public class SimplePlayerEvent : LuaEvent
    {
        public Player player;

        public SimplePlayerEvent(Player player)
        {
            this.player = player;
        }
    }
}
