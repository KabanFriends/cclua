using MCGalaxy.Commands;
using MCGalaxy.Network;
using MCGalaxy;
using BlockID = System.UInt16;

namespace CCLua.Commands
{
    public class CmdTempChunk : Command2
    {
        public override string name { get { return "tempchunk"; } }
        public override string shortcut { get { return "tempc"; } }
        public override string type { get { return CommandTypes.Building; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Guest; } }

        public override void Use(Player p, string message, CommandData data)
        {

            if (p.group.Permission < LevelPermission.Operator && !Hacks.CanUseHacks(p))
            {
                if (data.Context != CommandContext.MessageBlock)
                {
                    p.Message("%cYou cannot use this command manually when hacks are disabled.");
                    return;
                }
            }

            if (message == "") { Help(p); return; }
            string[] words = message.Split(' ');
            if (words.Length < 9)
            {
                p.Message("%cYou need to provide all 3 sets of coordinates, which means 9 numbers total.");
                return;
            }

            int x1 = 0, y1 = 0, z1 = 0, x2 = 0, y2 = 0, z2 = 0, x3 = 0, y3 = 0, z3 = 0;

            bool mistake = false;
            if (!CommandParser.GetInt(p, words[0], "x1", ref x1, 0, p.Level.Width - 1)) { mistake = true; }
            if (!CommandParser.GetInt(p, words[1], "y1", ref y1, 0, p.Level.Height - 1)) { mistake = true; }
            if (!CommandParser.GetInt(p, words[2], "z1", ref z1, 0, p.Level.Length - 1)) { mistake = true; }

            if (!CommandParser.GetInt(p, words[3], "x2", ref x2, 0, p.Level.Width - 1)) { mistake = true; }
            if (!CommandParser.GetInt(p, words[4], "y2", ref y2, 0, p.Level.Height - 1)) { mistake = true; }
            if (!CommandParser.GetInt(p, words[5], "z2", ref z2, 0, p.Level.Length - 1)) { mistake = true; }

            if (!CommandParser.GetInt(p, words[6], "x3", ref x3, 0, p.Level.Width - 1)) { mistake = true; }
            if (!CommandParser.GetInt(p, words[7], "y3", ref y3, 0, p.Level.Height - 1)) { mistake = true; }
            if (!CommandParser.GetInt(p, words[8], "z3", ref z3, 0, p.Level.Length - 1)) { mistake = true; }
            if (mistake) { return; }

            if (x1 > x2) { p.Message("%cx1 cannot be greater than x2!"); mistake = true; }
            if (y1 > y2) { p.Message("%cy1 cannot be greater than y2!"); mistake = true; }
            if (z1 > z2) { p.Message("%cz1 cannot be greater than z2!"); mistake = true; }
            if (mistake) { p.Message("%HMake sure the first set of coords is on the minimum corner (press f7)"); return; }
            bool allPlayers = false;
            if (words.Length > 9)
            {
                CommandParser.GetBool(p, words[9], ref allPlayers);
                if (data.Context != CommandContext.MessageBlock && allPlayers)
                {
                    p.Message("%cYou can't send the tempchunk to all players unless the command comes from a message block.");
                    return;
                }
            }

            BlockID[] blocks = GetBlocks(p, x1, y1, z1, x2, y2, z2);

            PlaceBlocks(p, blocks, x1, y1, z1, x2, y2, z2, x3, y3, z3, allPlayers);
        }

        public BlockID[] GetBlocks(Player p, int x1, int y1, int z1, int x2, int y2, int z2)
        {

            int xLen = (x2 - x1) + 1;
            int yLen = (y2 - y1) + 1;
            int zLen = (z2 - z1) + 1;

            BlockID[] blocks = new BlockID[xLen * yLen * zLen];
            int index = 0;

            for (int xi = x1; xi < x1 + xLen; ++xi)
            {
                for (int yi = y1; yi < y1 + yLen; ++yi)
                {
                    for (int zi = z1; zi < z1 + zLen; ++zi)
                    {
                        blocks[index] = p.level.GetBlock((ushort)xi, (ushort)yi, (ushort)zi);
                        index++;
                    }
                }
            }
            return blocks;
        }

        public void PlaceBlocks(Player p, BlockID[] blocks, int x1, int y1, int z1, int x2, int y2, int z2, int x3, int y3, int z3, bool allPlayers = false)
        {

            int xLen = (x2 - x1) + 1;
            int yLen = (y2 - y1) + 1;
            int zLen = (z2 - z1) + 1;

            Player[] players = allPlayers ? PlayerInfo.Online.Items : new[] { p };

            foreach (Player pl in players)
            {
                if (pl.level != p.level) continue;

                BufferedBlockSender buffer = new BufferedBlockSender(pl);
                int index = 0;
                for (int xi = x3; xi < x3 + xLen; ++xi)
                {
                    for (int yi = y3; yi < y3 + yLen; ++yi)
                    {
                        for (int zi = z3; zi < z3 + zLen; ++zi)
                        {
                            int pos = pl.level.PosToInt((ushort)xi, (ushort)yi, (ushort)zi);
                            if (pos >= 0) buffer.Add(pos, blocks[index]);
                            index++;
                        }
                    }
                }
                // last few blocks 
                buffer.Flush();
            }

        }

        public override void Help(Player p)
        {
            p.Message("%T/TempChunk %f[x1 y1 z1] %7[x2 y2 z2] %r[x3 y3 z3] <true/false>");
            p.Message("%HCopies a chunk of the world defined by %ffirst %Hand %7second%H coords then pastes it into the spot defined by the %rthird %Hset of coords.");
            p.Message("%HThe last option is optional, and defaults to false. If true, the tempchunk changes are sent to all players in the map.");
        }

    }
}
