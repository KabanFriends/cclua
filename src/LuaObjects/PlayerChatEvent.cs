using MCGalaxy;

namespace CCLua.LuaObjects
{
    public class PlayerChatEvent : SimplePlayerEvent
    {
        public string message;

        public PlayerChatEvent(Player p, string message) : base(p)
        {
            this.message = message;
        }
    }
}
