namespace CCLua.LuaObjects
{
    public class LuaEvent
    {
        public CancelState cancelState = CancelState.UNCHANGED;

        public void Cancel()
        {
            cancelState = CancelState.CANCELLED;
        }

        public void Uncancel()
        {
            cancelState = CancelState.UNCANCELLED;
        }
    }
}
