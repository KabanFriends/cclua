namespace CCLua
{
    public class OSLuaConfiguration : LuaConfiguration
    {
        public override int instructionLimit => 20000;

        public override int executionCheckTimeMs => 5;

        public override int instructionsPerExecutionCheck => 10;

        public override int instantExecutionTimeNanos => 200 * 1000000;
    }
}
