using MCGalaxy;

namespace CCLua.LuaObjects
{
    public class PlayerMoveEvent : SimplePlayerEvent
    {
        public double x;

        public double y;

        public double z;

        public double yaw;

        public double pitch;

        public PlayerMoveEvent(Player p, Position pos, byte yaw, byte pitch) : base(p)
        {
            x = pos.X / 32d - 0.5d;
            y = pos.Y / 32d - 0.5d;
            z = pos.Z / 32d - 0.5d;
            this.yaw = GetYaw(yaw);
            this.pitch = GetPitch(pitch);
        }

        private static double GetYaw(int input)
        {
            if (input > 128)
            {
                input -= 256;
            }

            return input / 256d * 360;
        }

        private static double GetPitch(int input)
        {
            if (input > 128)
            {
                input -= 256;
            }

            return input / 256d * 360;
        }
    }
}
