using MCGalaxy;

namespace CCLua.LuaObjects
{
    public class PlayerBlockChangeEvent : SimplePlayerEvent
    {
        public ushort x;

        public ushort y;

        public ushort z;

        public ushort block;

        public PlayerBlockChangeEvent(Player p, ushort x, ushort y, ushort z, ushort block) : base (p)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.block = block;
        }
    }
}
