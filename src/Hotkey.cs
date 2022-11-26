namespace CCLua
{
    public class Hotkey
    {
        public int keyCode;
        public byte modifiers;

        public Hotkey(int keyCode, byte modifiers)
        {
            this.keyCode = keyCode;
            this.modifiers = modifiers;
        }
    }
}
