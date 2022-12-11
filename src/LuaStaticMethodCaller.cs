using MCGalaxy;
using System;
using System.Reflection;

namespace CCLua
{
    public class LuaStaticMethodCaller
    {
        private LuaContext context;

        public LuaStaticMethodCaller(LuaContext context)
        {
            this.context = context;
        }

        public object Call(string typeFullName, string method, params object[] args)
        {
            Player p = context.currentPlayer;
            if (args.Length > 0)
            {
                if (args[0] is Player player)
                {
                    p = player;
                }
            }

            try
            {
                return Assembly.GetExecutingAssembly().GetType(typeFullName).GetMethod(method).Invoke(null, args);
            } catch (Exception e)
            {
                if (p != null)
                {
                    string extraInfo = "";

                    if (e is TargetInvocationException te)
                    {
                        if (te.InnerException != null && te.InnerException is UserScriptException ue)
                        {
                            extraInfo = "(" + ue.Message + ")";
                        }
                    }

                    var methodName = char.ToLower(method[0]) + method.Substring(1);

                    p.Message($"&cError: Invalid usage of {methodName} {extraInfo}");

                    if (p.Rank > LevelPermission.Operator)
                    {
                        Logger.LogError(e);
                        p.Message("&cThe error has been logged in the console.");
                    }
                }
                return null;
            }
        }
    }
}
