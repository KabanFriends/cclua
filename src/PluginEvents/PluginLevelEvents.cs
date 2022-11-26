using MCGalaxy;

namespace CCLua.PluginEvents
{
    public static class PluginLevelEvents
    {
        public static void OnLevelLoaded(Level level)
        {
            if (LevelHandler.HasLuaContext(level))
            {
                //should not happen, but just in case
                LevelHandler.StopLuaContext(level);
            }

            if (LevelHandler.TryCreateLuaContext(level))
            {
                LuaContext context = LevelHandler.GetContextByLevel(level);
                context.Call("onLevelStart");
            }
        }

        public static void OnLevelUnload(Level level, ref bool cancel)
        {
            if (LevelHandler.HasLuaContext(level))
            {
                LuaContext context = LevelHandler.GetContextByLevel(level);

                if (context.stopped)
                {
                    return;
                }

                context.Call("onLevelStop");
                LevelHandler.StopLuaContext(level);
            }
        }
    }
}
