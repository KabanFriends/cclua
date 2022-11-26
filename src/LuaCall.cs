using CCLua.LuaObjects.Suppliers;
using MCGalaxy;
using NLua;
using System;

namespace CCLua
{
    public class LuaCall
    {
        public string functionName;
        public string playerName;
        public object[] args;

        public LuaCall(string function, Player player, params object[] args)
        {
            this.functionName = function;
            this.args = args;

            if (player != null)
            {
                this.playerName = player.truename;
            }
        }

        public bool IsObjectSupplier(int index)
        {
            return args[index] is LuaObjectSupplier;
        }
    }
}
