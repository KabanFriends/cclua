namespace CCLua
{
    public class StaffLevelLuaConfiguration : LuaConfiguration
    {
        public override int instructionLimit => 50000;

        public override int executionCheckTimeMs => 5;

        public override int instructionsPerExecutionCheck => 50;

        public override long instantExecutionTimeNanos => 10000 * 1000000L;

        public override int storageMaxSize => 50_000_000; //50MB
    }
}
