namespace CCLua
{
    public class OSLuaConfiguration : LuaConfiguration
    {
        public override int instructionLimit => 20000;

        public override int executionCheckTimeMs => 5;

        public override int instructionsPerExecutionCheck => 50;

        public override long instantExecutionTimeNanos => 1000 * 1000000L;

        public override int storageMaxSize => 5_000_000; //5MB 
    }
}
