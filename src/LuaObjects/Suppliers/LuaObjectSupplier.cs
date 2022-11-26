using NLua;

namespace CCLua.LuaObjects.Suppliers
{
    public abstract class LuaObjectSupplier
    {
        //dummy class for type checks
    }

    public abstract class LuaObjectSupplier<T> : LuaObjectSupplier
    {
        public T value; 

        public LuaObjectSupplier(T obj)
        {
            value = obj;
        }

        //called from lua, very janky but this works
        public LuaTable CallSupply(Lua lua)
        {
            return (LuaTable) GetType().GetMethod("Supply").Invoke(null, new object[] { lua, value });
        }

        public static LuaTable Supply(Lua lua, T obj)
        {
            return null;
        }
    }
}
