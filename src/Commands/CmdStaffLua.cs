using MCGalaxy;
using System.Linq;

namespace CCLua.Commands
{
    public class CmdStaffLua : Command2
    {
        public override string name => "StaffLua";

        public override string type => CommandTypes.Other;

        public override LevelPermission defaultRank => LevelPermission.Operator;

        public override void Use(Player p, string message, CommandData data)
        {
            bool result;
            Level level;
            if (message.Length == 0)
            {
                level = p.level;
            } else
            {
                string[] args = message.SplitSpaces();
                level = Matcher.FindLevels(p, args[0]);
                if (level == null)
                {
                    return;
                }
            }

            result = ToggleStaffLua(level);
            if (result)
            {
                p.Message("Staff lua mode is now &aON&S for the level &b{0}&S.", level.name);
            } else
            {
                p.Message("Staff lua mode is now &cOFF&S for the level &b{0}&S.", level.name);
            }
            p.Message("Type &a/lua reload&S to apply the change.");
        }

        public override void Help(Player p)
        {
            p.Message("&T/StaffLua");
            p.Message("&HToggles the staff lua mode for the current level.");
            p.Message("&T/StaffLua [level]");
            p.Message("&HToggles the staff lua mode for the specified level.");
        }

        private static bool ToggleStaffLua(Level level)
        {
            bool result = false;
            if (CCLuaPlugin.staffMaps.Contains(level.name))
            {
                CCLuaPlugin.staffMaps.Remove(level.name);
            }
            else
            {
                CCLuaPlugin.staffMaps.Add(level.name);
                result = true;
            }

            CCLuaPlugin.staffMapsFile.SetText(CCLuaPlugin.staffMaps.ToArray());
            return result;
        }
    }
}
