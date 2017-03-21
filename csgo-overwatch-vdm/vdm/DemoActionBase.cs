namespace csgo_overwatch_vdm.vdm
{
    public abstract class DemoActionBase
    {
        public string Factory { get; set; }
        public string Name { get; set; }
        public int StartTick { get; set; }

        public abstract string ToString(int index);
    }
}