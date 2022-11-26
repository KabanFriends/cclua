using MCGalaxy;

namespace CCLua.LuaObjects
{
    public class PlayerCallFunctionEvent : SimplePlayerEvent
    {
        public int? mbX;

        public int? mbY;

        public int? mbZ;

        public PlayerCallFunctionEvent(Player p, int? mbX, int? mbY, int? mbZ) : base(p)
        {
            this.mbX = mbX;
            this.mbY = mbY;
            this.mbZ = mbZ;
        }
    }
}
