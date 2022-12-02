namespace CCLua
{
    public class StaffLevelLuaConfiguration : LuaConfiguration
    {
        public override int instructionLimit => 50000;

        public override int executionCheckTimeMs => 5;

        public override int instructionsPerExecutionCheck => 10;

        public override int instantExecutionTimeNanos => 200 * 1000000;

        public override int storageMaxSize => 20_000_000; //20MB 
    }
}
