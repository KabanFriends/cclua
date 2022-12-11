using NLua;
using MCGalaxy;
using System;
using CCLua.LuaObjects;

namespace CCLua
{
    public class LuaSchedule
    {
        public LuaPlayer luaPlayer;
        public long waitUntil;
        public object coroutine;

        public LuaSchedule(object function, long waitUntil, LuaPlayer luaPlayer)
        {
            this.luaPlayer = luaPlayer;
            this.waitUntil = Environment.TickCount + waitUntil;
            this.coroutine = function;
        }
    }
}
