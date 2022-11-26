using fNbt;
using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;
using System;
using System.Linq;

namespace CCLua.LuaObjects
{
    public class PlayerClickEvent : SimplePlayerEvent
    {
        public double yaw;

        public double pitch;

        public int? entityId = null;

        public ushort? x = null;

        public ushort? y = null;

        public ushort? z = null;

        public TargetBlockFace? face = null;

        public PlayerClickEvent(Player player, ushort yaw, ushort pitch, ushort entityId, ushort x, ushort y, ushort z, TargetBlockFace face) : base(player)
        {
            this.yaw = GetYaw(yaw);
            this.pitch = GetPitch(pitch);
            if (entityId != 255) this.entityId = entityId;
            if (x != 65535) this.x = x;
            if (y != 65535) this.y = y;
            if (z != 65535) this.z = z;
            if (face != TargetBlockFace.None) this.face = face;
        }

        public string GetFace()
        {
            if (face == null) return null;

            var str = face.ToString();
            return char.ToLower(str[0]) + str.Substring(1);
        }

        private static double GetYaw(double input)
        {
            if (input > 32768)
            {
                input -= 65536;
            }

            return input / 65536d * 360;
        }

        private static double GetPitch(double input)
        {
            if (input > 32768)
            {
                input -= 65536;
            }

            return input / 65536d * 360;
        }
    }
}
