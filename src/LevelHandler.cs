using MCGalaxy;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace CCLua
{
    public static class LevelHandler
    {
        private static readonly LuaConfiguration staffConfig = new StaffLevelLuaConfiguration();
        private static readonly LuaConfiguration osConfig = new OSLuaConfiguration();

        public static Dictionary<string, LuaContext> contexts = new Dictionary<string, LuaContext>();

        public static bool TryCreateLuaContext(Level level)
        {
            string path = Constants.CCLUA_BASE_DIR + Constants.SCRIPT_DIR + level.name + ".lua";

            if (File.Exists(path))
            {
                Logger.Log(LogType.BackgroundActivity, "cclua: Creating a new lua context for level " + level.name);

                LuaContext context = new LuaContext(level);
                contexts.Add(level.name, context);

                context.config = LevelUtil.IsOsLevel(level) ? osConfig : staffConfig;

                context.LoadLua();

                return true;
            }

            return false;
        }

        public static void StopLuaContext(Level level)
        {
            LuaContext context = GetContextByLevel(level);
            ThreadStart ts = delegate
            {
                Thread.Sleep(10);
                context.Stop();
            };

            Thread t = new Thread(ts);
            t.Start();

            contexts.Remove(level.name);
        }

        public static LuaContext GetContextByLevel(Level level)
        {
            return contexts.ContainsKey(level.name) ? contexts[level.name] : null;
        }

        public static bool HasLuaContext(Level level)
        {
            return contexts.ContainsKey(level.name);
        }
    }
}
