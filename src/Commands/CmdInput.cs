using MCGalaxy;

namespace CCLua.Commands
{
    public class CmdInput : Command2
    {
        public override string name { get { return "Input"; } }
        public override string type { get { return CommandTypes.Other; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Guest; } }

        public override void Use(Player p, string message, CommandData data)
        {
            //dummy. actual input is handled in an event for compatibility with other plugins
        }

        public override void Help(Player p)
        {
            p.Message("%T/Input");
            p.Message("%HText input supplement for adventure maps.");
        }
    }
}
