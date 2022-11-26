using MCGalaxy;

namespace CCLua.LuaObjects
{
    public class PlayerInputEvent : SimplePlayerEvent
    {
        public string message;

        public string[] args;

        public PlayerInputEvent(Player p, string message) : base(p)
        {
            this.message = message;
            args = message.SplitSpaces();
        }
    }
}
