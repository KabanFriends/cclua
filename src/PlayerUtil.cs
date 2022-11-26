using MCGalaxy;
using MCGalaxy.Commands;
using MCGalaxy.Maths;
using MCGalaxy.Network;
using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using BlockID = System.UInt16;

namespace CCLua
{
    public static class PlayerUtil
    {
        private static readonly Dictionary<string, int> keyCodes = new Dictionary<string, int>
        {
            { "NONE",         0   },
            { "ESCAPE",       1   },
            { "1",            2   },
            { "2",            3   },
            { "3",            4   },
            { "4",            5   },
            { "5",            6   },
            { "6",            7   },
            { "7",            8   },
            { "8",            9   },
            { "9",            10  },
            { "0",            11  },
            { "MINUS",        12  },
            { "EQUALS",       13  },
            { "BACK",         14  },
            { "TAB",          15  },
            { "Q",            16  },
            { "W",            17  },
            { "E",            18  },
            { "R",            19  },
            { "T",            20  },
            { "Y",            21  },
            { "U",            22  },
            { "I",            23  },
            { "O",            24  },
            { "P",            25  },
            { "LBRACKET",     26  },
            { "RBRACKET",     27  },
            { "RETURN",       28  },
            { "LCONTROL",     29  },
            { "A",            30  },
            { "S",            31  },
            { "D",            32  },
            { "F",            33  },
            { "G",            34  },
            { "H",            35  },
            { "J",            36  },
            { "K",            37  },
            { "L",            38  },
            { "SEMICOLON",    39  },
            { "APOSTROPHE",   40  },
            { "GRAVE",        41  },
            { "LSHIFT",       42  },
            { "BACKSLASH",    43  },
            { "Z",            44  },
            { "X",            45  },
            { "C",            46  },
            { "V",            47  },
            { "B",            48  },
            { "N",            49  },
            { "M",            50  },
            { "COMMA",        51  },
            { "PERIOD",       52  },
            { "SLASH",        53  },
            { "RSHIFT",       54  },
            { "MULTIPLY",     55  },
            { "LMENU",        56  },
            { "SPACE",        57  },
            { "CAPITAL",      58  },
            { "F1",           59  },
            { "F2",           60  },
            { "F3",           61  },
            { "F4",           62  },
            { "F5",           63  },
            { "F6",           64  },
            { "F7",           65  },
            { "F8",           66  },
            { "F9",           67  },
            { "F10",          68  },
            { "NUMLOCK",      69  },
            { "SCROLL",       70  },
            { "NUMPAD7",      71  },
            { "NUMPAD8",      72  },
            { "NUMPAD9",      73  },
            { "SUBTRACT",     74  },
            { "NUMPAD4",      75  },
            { "NUMPAD5",      76  },
            { "NUMPAD6",      77  },
            { "ADD",          78  },
            { "NUMPAD1",      79  },
            { "NUMPAD2",      80  },
            { "NUMPAD3",      81  },
            { "NUMPAD0",      82  },
            { "DECIMAL",      83  },
            { "F11",          87  },
            { "F12",          88  },
            { "F13",          100 },
            { "F14",          101 },
            { "F15",          102 },
            { "F16",          103 },
            { "F17",          104 },
            { "F18",          105 },
            { "KANA",         112 },
            { "F19",          113 },
            { "CONVERT",      121 },
            { "NOCONVERT",    123 },
            { "YEN",          125 },
            { "NUMPADEQUALS", 141 },
            { "CIRCUMFLEX",   144 },
            { "AT",           145 },
            { "COLON",        146 },
            { "UNDERLINE",    147 },
            { "KANJI",        148 },
            { "STOP",         149 },
            { "AX",           150 },
            { "UNLABELED",    151 },
            { "NUMPADENTER",  156 },
            { "RCONTROL",     157 },
            { "SECTION",      167 },
            { "NUMPADCOMMA",  179 },
            { "DIVIDE",       181 },
            { "SYSRQ",        183 },
            { "RMENU",        184 },
            { "FUNCTION",     196 },
            { "PAUSE",        197 },
            { "HOME",         199 },
            { "UP",           200 },
            { "PRIOR",        201 },
            { "LEFT",         203 },
            { "RIGHT",        205 },
            { "END",          207 },
            { "DOWN",         208 },
            { "NEXT",         209 },
            { "INSERT",       210 },
            { "DELETE",       211 },
            { "CLEAR",        218 },
            { "LMETA",        219 },
            { "RMETA",        220 },
            { "APPS",         221 },
            { "POWER",        222 },
            { "SLEEP",        223 }
        };

        public static void CpeMessage(Player p, string pos, string message)
        {
            CpeMessageType type;
            switch (pos.ToLower())
            {
                case "top1":
                    type = CpeMessageType.Status1;
                    break;
                case "top2":
                    type = CpeMessageType.Status2;
                    break;
                case "top3":
                    type = CpeMessageType.Status3;
                    break;
                case "bottom1":
                    type = CpeMessageType.BottomRight1;
                    break;
                case "bottom2":
                    type = CpeMessageType.BottomRight2;
                    break;
                case "bottom3":
                    type = CpeMessageType.BottomRight3;
                    break;
                case "announce":
                    type = CpeMessageType.Announcement;
                    break;
                case "bigannounce":
                    type = CpeMessageType.BigAnnouncement;
                    break;
                case "smallannounce":
                    type = CpeMessageType.SmallAnnouncement;
                    break;
                default:
                    type = CpeMessageType.Normal;
                    break;
            }

            if (message.Length > 64)
            {
                message = message.Substring(0, 64);
            }

            p.SendCpeMessage(type, message);
        }

        public static void Kill(Player p, string msg)
        {
            p.HandleDeath(Block.Cobblestone, msg, false, true);
        }

        public static void Cmd(Player p, string command)
        {
            List<string> strs = command.Split(' ').ToList();
            string cmdName = strs[0];
            strs.RemoveAt(0);
            string cmdArgs = string.Join(" ", strs);

            Command.Search(ref cmdName, ref cmdArgs);
            if (cmdName.CaselessEq("lua"))
            {
                p.Message("&cCommand \"{0}\" is blacklisted from being used in scripts.", cmdName);
                return;
            }

            Command cmd = Command.Find(cmdName);
            if (cmd == null)
            {
                p.Message("&cCould not find command \"{0}\".", cmdName);
                return;
            }

            RunCmd(p, cmd, cmdArgs);
        }

        public static void RunCmd(Player p, Command cmd, string args)
        {
            CommandData data = GetCommandData(p);
            if (LevelUtil.IsOsLevel(p.level) && cmd.MessageBlockRestricted)
            {
                p.Message("&c\"{0}\" cannot be used in message blocks.", cmd.name);
                p.Message("&cTherefore, it cannot be ran in a script.");
                return;
            }
            CommandPerms perms = CommandPerms.Find(cmd.name);
            if (!perms.UsableBy(data.Rank))
            {
                p.Message("&cOS lua scripts can only run commands with a permission of member or lower.", data.Rank);
                p.Message("&cTherefore, \"{0}\" cannot be ran.", cmd.name);
                return;
            }
            try
            {
                cmd.Use(p, args, data);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                p.Message("&cAn error occured and command {0} could not be executed.", cmd.name);
            }
        }

        public static CommandData GetCommandData(Player p)
        {
            CommandData data = default(CommandData);
            data.Context = CommandContext.MessageBlock;
            data.Rank = LevelUtil.IsOsLevel(p.level) ? LevelPermission.Guest : LevelPermission.Nobody;
            return data;
        }

        public static double GetYaw(Player p)
        {
            double yaw = p.Rot.RotY;
            if (yaw > 128)
            {
                yaw -= 256;
            }

            return yaw / 256d * 360;
        }

        public static double GetPitch(Player p)
        {
            double pitch = p.Rot.HeadX;
            if (pitch > 128)
            {
                pitch -= 256;
            }

            return pitch / 256d * 360;
        }

        public static void TempBlock(Player p, object block, double x, double y, double z)
        {
            string args = "";

            if (block is string str)
            {
                args += str;
            } else if (block is double d)
            {
                args += (int)d;
            } else
            {
                throw new UserScriptException("Invalid block type in tempblock");
            }

            args += $" {x} {y} {z}";

            RunCmd(p, CCLuaPlugin.tempBlockToUse, args);
        }

        public static void TempChunk(Player p, double x1, double y1, double z1, double x2, double y2, double z2, double x3, double y3, double z3, bool toAll)
        {
            RunCmd(p, CCLuaPlugin.tempChunkToUse, $"{(int)x1} {(int)y1} {(int)z1} {(int)x2} {(int)y2} {(int)z2} {(int)x3} {(int)y3} {(int)z3} {toAll}");
        }

        public static void Freeze(Player p)
        {
            p.Extras["lua_frozen"] = true;
            p.Send(Packet.Motd(p, "-hax horspeed=0.000001 jumps=0 -push"));
        }

        public static void Unfreeze(Player p)
        {
            p.Extras["lua_frozen"] = false;

            PlayerData data = LevelHandler.GetContextByLevel(p.level).GetPlayerData(p);
            if (data.customMotd != null)
            {
                SendMotd(p, data.customMotd);
                return;
            }
            SendMotd(p, p.GetMotd());
        }

        public static void SendMotd(Player p, string motd)
        {
            p.Send(Packet.Motd(p, motd));
            if (p.Supports(CpeExt.HackControl))
            {
                p.Send(Hacks.MakeHackControl(p, motd));
            }
        }

        public static void Look(Player p, double x, double y, double z)
        {
            Vec3S32 coords = new Vec3S32((int)x, (int)y, (int)z);
            LookAtCoords(p, coords);
        }

        public static void StartStare(Player p, double x, double y, double z)
        {
            Vec3S32 coords = new Vec3S32((int)x, (int)y, (int)z);
            LevelHandler.GetContextByLevel(p.level).GetPlayerData(p).stareAt = coords;
            LookAtCoords(p, coords);
        }

        public static void StopStare(Player p)
        {
            LevelHandler.GetContextByLevel(p.level).GetPlayerData(p).stareAt = null;
        }

        public static void LookAtCoords(Player p, Vec3S32 coords)
        {
            //we want to calculate difference between player's (eye position)
            //and block's position to use in GetYawPitch

            //convert block coords to player units
            coords *= 32;
            //center of the block
            coords += new Vec3S32(16, 16, 16);

            int dx = coords.X - p.Pos.X;
            int dy = coords.Y - (p.Pos.Y - Entities.CharacterHeight + ModelInfo.CalcEyeHeight(p));
            int dz = coords.Z - p.Pos.Z;
            Vec3F32 dir = new Vec3F32(dx, dy, dz);
            dir = Vec3F32.Normalise(dir);

            byte yaw, pitch;
            DirUtils.GetYawPitch(dir, out yaw, out pitch);
            byte[] packet = new byte[4];
            packet[0] = Opcode.OrientationUpdate; packet[1] = Entities.SelfID; packet[2] = yaw; packet[3] = pitch;
            p.Send(packet);
        }

        public static byte? GetEnvColorType(string type)
        {
            if (type.CaselessEq("sky")) { return 0; }
            if (type.CaselessEq("cloud")) { return 1; }
            if (type.CaselessEq("clouds")) { return 1; }
            if (type.CaselessEq("fog")) { return 2; }
            if (type.CaselessEq("shadow")) { return 3; }
            if (type.CaselessEq("sun")) { return 4; }
            if (type.CaselessEq("skybox")) { return 5; }
            return null;
        }
        public static byte? GetEnvWeatherType(string type)
        {
            if (type.CaselessEq("sun")) { return 0; }
            if (type.CaselessEq("rain")) { return 1; }
            if (type.CaselessEq("snow")) { return 2; }
            return null;
        }
        public static EnvProp? GetEnvMapProperty(string prop)
        {
            if (prop.CaselessEq("maxfog")) { return EnvProp.MaxFog; }
            if (prop.CaselessEq("expfog")) { return EnvProp.ExpFog; }
            if (prop.CaselessEq("cloudsheight") || prop.CaselessEq("cloudheight")) { return EnvProp.CloudsLevel; }
            if (prop.CaselessEq("cloudspeed") || prop.CaselessEq("cloudspeed")) { return EnvProp.CloudsSpeed; }
            return null;
        }

        public static void SetEnv(Player p, string prop, object value)
        {
            byte? type = GetEnvColorType(prop);
            if (type != null)
            {
                string hex;
                if (value is double || value is int)
                {
                    hex = string.Format("{0:X}", value);
                } else if (value is string stringValue)
                {
                    hex = stringValue;
                } else
                {
                    throw new UserScriptException("Env color can only be a string or a number");
                }

                p.Session.SendSetEnvColor((byte)type, hex);
                return;
            }
            if (prop.CaselessEq("weather"))
            {
                if (!(value is string)) {
                    throw new UserScriptException("Env weather type can only be a string");
                }

                type = GetEnvWeatherType((string)value);
                if (type != null)
                {
                    p.Session.SendSetWeather((byte)type);
                }
                return;
            }

            EnvProp? envPropType = GetEnvMapProperty(prop);
            if (envPropType != null)
            {
                if (envPropType == EnvProp.ExpFog)
                {
                    bool yesno = false;
                    if (value is bool)
                    {
                        yesno = (bool)value;
                    } else if (value is string valueString) {
                        if (!CommandParser.GetBool(p, valueString, ref yesno))
                        {
                            return;
                        }
                    } else
                    {
                        throw new UserScriptException("Env expfog can only be a boolean or a string");
                    }

                    p.Send(Packet.EnvMapProperty((EnvProp)envPropType, yesno ? 1 : 0));
                    return;
                }

                int propValue = 0;
                if (value is double doubleValue)
                {
                    propValue = (int)doubleValue;
                } else if (value is int intValue)
                {
                    propValue = intValue;
                } else if (value is string valueString)
                {
                    if (!CommandParser.GetInt(p, valueString, "env int value", ref propValue))
                    {
                        return;
                    }
                } else
                {
                    throw new UserScriptException("Env properties can only be a number or a string");
                }

                if (envPropType == EnvProp.CloudsSpeed) { propValue *= 256; }
                p.Send(Packet.EnvMapProperty((EnvProp)envPropType, propValue));
                return;
            }
        }

        public static void Motd(Player p, string motd)
        {
            PlayerData data = LevelHandler.GetContextByLevel(p.level).GetPlayerData(p);
            if (motd == null || motd.CaselessEq("ignore"))
            {
                data.customMotd = motd;
                SendMotd(p, p.GetMotd());
                return;
            }

            data.customMotd = motd;
            SendMotd(p, motd);
        }

        public static void SetSpawn(Player p, double x, double y, double z)
        {
            Position pos = new Position();
            pos.X = (int)(x * 32d);
            pos.Y = (int)(y * 32d) + Entities.CharacterHeight;
            pos.Z = (int)(z * 32d);

            if (p.Supports(CpeExt.SetSpawnpoint))
            {
                p.Send(Packet.SetSpawnpoint(pos, p.Rot, p.Supports(CpeExt.ExtEntityPositions)));
            }
            else
            {
                p.SendPos(Entities.SelfID, pos, p.Rot);
                Entities.Spawn(p, p);
            }
            p.Message("Your spawnpoint was updated.");
        }

        public static string GetTrueMotd(Player p)
        {
            PlayerData data = LevelHandler.GetContextByLevel(p.level).GetPlayerData(p);

            if (data.customMotd != null)
            {
                return data.customMotd;
            }
            return p.GetMotd();
        }

        public static void AddHotkey(Player p, string input, string key, LuaTable table)
        {
            List<string> list = new List<string>();
            foreach (object obj in table.Values)
            {
                list.Add((string)obj);
            }

            DefineHotkey(p, input, key, list.ToArray());
        }

        public static void DefineHotkey(Player p, string input, string key, params string[] modifiers)
        {
            int keyCode = GetKeyCode(key);
            if (keyCode == 0)
            {
                throw new UserScriptException($"Invalid key name {key}! Please refer to https://minecraft.fandom.com/el/wiki/Key_codes#Full_table for a list of key names.");
            }

            byte modifierData = GetModifiers(modifiers);

            PlayerData data = LevelHandler.GetContextByLevel(p.level).GetPlayerData(p);
            data.hotkeys.Add($"{keyCode}+{modifierData}", new Hotkey(keyCode, modifierData));

            string cmd = $"/input {input}\n";
            if (cmd.Length > NetUtils.StringSize)
            {
                p.Message($"&cThe hotkey input message is too long. Max length is {NetUtils.StringSize - 7} characters.");
                return;
            }

            p.Send(Packet.TextHotKey("cclua hotkey", $"/input {input}\n", keyCode, modifierData, true));
        }

        public static void RemoveHotkey(Player p, string key, LuaTable table)
        {
            List<string> list = new List<string>();
            foreach (object obj in table.Values)
            {
                list.Add((string)obj);
            }

            UndefineHotkey(p, key, list.ToArray());
        }

        public static void UndefineHotkey(Player p, string key, params string[] modifiers)
        {
            int keyCode = GetKeyCode(key);
            if (keyCode == 0)
            {
                throw new UserScriptException($"Invalid key name {key}! Please refer to https://minecraft.fandom.com/el/wiki/Key_codes#Full_table for a list of key names.");
            }

            byte modifierData = GetModifiers(modifiers);

            PlayerData data = LevelHandler.GetContextByLevel(p.level).GetPlayerData(p);
            if (data.hotkeys.ContainsKey($"{keyCode}+{modifierData}"))
            {
                Hotkey hotkey = data.hotkeys[$"{keyCode}+{modifierData}"];
                data.hotkeys.Remove($"{keyCode}+{modifierData}");
                UndefineHotkey(p, hotkey);
            }
        }

        public static void UndefineHotkey(Player p, Hotkey hotkey)
        {
            p.Send(Packet.TextHotKey("", "", hotkey.keyCode, hotkey.modifiers, true));
        }

        public static int GetKeyCode(string keyName)
        {
            int code = 0;
            keyCodes.TryGetValue(keyName.ToUpper(), out code);
            return code;
        }

        public static byte GetModifiers(string[] modifiers)
        {
            byte ctrlFlag = (byte)(modifiers.CaselessContains("ctrl") ? 1 : 0);
            byte shiftFlag = (byte)(modifiers.CaselessContains("shift") ? 2 : 0);
            byte altFlag = (byte)(modifiers.CaselessContains("alt") ? 4 : 0);
            return (byte)(ctrlFlag | shiftFlag | altFlag);
        }

        public static void SetReach(Player p, double reach)
        {
            int packedDist = (int)(reach * 32d);
            if (packedDist > short.MaxValue)
            {
                p.Message("&c&cReach of \"{0}\", is too long. Max reach is 1023 blocks.", reach);
                return;
            }

            p.Send(Packet.ClickDistance((short)packedDist));
        }

        public static void SetModel(Player p, string model)
        {
            PlayerData data = LevelHandler.GetContextByLevel(p.level).GetPlayerData(p);

            if (model == null)
            {
                p.UpdateModel(data.model);
            }

            p.UpdateModel(model);
        }

        public static void Launch(Player p, double x, double y, double z, bool xAdd, bool yAdd, bool zAdd)
        {
            p.Send(Packet.VelocityControl((float)x, (float)y, (float)z, Convert.ToByte(!xAdd), Convert.ToByte(!yAdd), Convert.ToByte(!zAdd)));
        }

        public static void SetHotbar(Player p, double slot, object block)
        {
            if (slot < 1 || slot > 9)
            {
                throw new UserScriptException("Slot number must be between 1 and 9");
            }

            string b = null;
            if (block is string strBlock)
            {
                b = strBlock;
            } else if (block is double dbl)
            {
                b = dbl.ToString();
            }

            if (b == null) return;

            BlockID id = Block.Parse(p, b);
            p.Send(Packet.SetHotbar(id, Convert.ToByte(slot - 1), p.Session.hasExtBlocks));
        }

        public static void Teleport(Player p, double x, double y, double z)
        {
            DoTeleport(p, x, y, z, null, null);
        }

        public static void TeleportRot(Player p, double x, double y, double z, double yaw, double pitch)
        {
            DoTeleport(p, x, y, z, yaw, pitch);
        }

        public static void DoTeleport(Player p, double x, double y, double z, double? yaw, double? pitch)
        {
            p.PreTeleportMap = p.level.name;
            p.PreTeleportPos = p.Pos;
            p.PreTeleportRot = p.Rot;

            Position pos = new Position();
            pos.X = (int)(x * 32d);
            pos.Y = (int)(y * 32d) + Entities.CharacterHeight;
            pos.Z = (int)(z * 32d);

            Orientation rot;
            if (yaw == null || pitch == null)
            {
                rot = p.Rot;
            }
            else
            {
                rot = new Orientation((byte)(yaw * 256d / 360d), (byte)(pitch * 256d / 360d));
            }

            p.SendPosition(pos, rot);
        }

        public static bool IsUsingCef(Player p)
        {
            return p.appName == null ? false : p.appName.Contains("+ cef");
        }

        public static bool IsUsingMobile(Player p)
        {
            return p.appName == null ? false : p.appName.CaselessContains("mobile") || p.appName.CaselessContains("android");
        }

        public static bool IsUsingWeb(Player p)
        {
            return p.appName == null ? false : p.appName.CaselessContains("web");
        }
    }
}
