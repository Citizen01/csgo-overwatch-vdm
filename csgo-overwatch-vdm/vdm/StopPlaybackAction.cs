namespace csgo_overwatch_vdm.vdm
{
    internal class StopPlaybackAction : DemoActionBase
    {
        public StopPlaybackAction()
        {
            Factory = Name = "StopPlaybackAction";
        }

        private static string _template = @"
    ""{0}""
    {{
        factory ""{1}""
        name ""{2}""
        starttick ""{3}""
    }}";

        public override string ToString(int index)
        {
            return string.Format(_template, index, Factory, Name, StartTick);
        }
    }
}