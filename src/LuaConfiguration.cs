namespace CCLua
{
    public abstract class LuaConfiguration
    {
        public abstract int instructionLimit
        {
            get;
        }

        public abstract int executionCheckTimeMs
        {
            get;
        }

        public abstract int instructionsPerExecutionCheck
        {
            get;
        }

        public abstract int instantExecutionTimeNanos
        {
            get;
        }

        public abstract int storageMaxSize
        {
            get;
        }
    }
}
