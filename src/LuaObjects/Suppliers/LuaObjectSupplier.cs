using NLua;

namespace CCLua.LuaObjects.Suppliers
{
    public abstract class LuaObjectSupplier
    {
        //dummy class for type checks

        public abstract LuaTable CallSupply(LuaContext context);
    }

    public abstract class LuaObjectSupplier<T> : LuaObjectSupplier
    {
        public T value; 

        public LuaObjectSupplier(T obj)
        {
            value = obj;
        }

        //called from lua, very janky but this works
        public override LuaTable CallSupply(LuaContext context)
        {
            return (LuaTable) GetType().GetMethod("Supply").Invoke(null, new object[] { context, value });
        }

        public static LuaTable Supply(LuaContext context, T obj)
        {
            return null;
        }
    }
}
