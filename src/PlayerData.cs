using MCGalaxy;
using MCGalaxy.Maths;
using System.Collections.Generic;

namespace CCLua
{
    public class PlayerData
    {
        private Player player;

        public string customMotd;

        public Vec3S32? stareAt;

        public Dictionary<string, Hotkey> hotkeys;

        public string model;

        public PlayerData(Player player)
        {
            this.player = player;
            hotkeys = new Dictionary<string, Hotkey>();
            model = player.Model;
        }
    }
}
