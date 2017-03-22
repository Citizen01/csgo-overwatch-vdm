namespace csgo_overwatch_vdm.vdm
{
    internal class PlayCommandsAction : DemoActionBase
    {
        public string Commands { get; set; }

        public PlayCommandsAction()
        {
            Factory = Name = "PlayCommands";
        }

        private static string _template = @"
    ""{0}""
    {{
        factory ""{1}""
        name ""{2}""
        starttick ""{3}""
        commands ""{4}""
    }}";

        public override string ToString(int index)
        {
            return string.Format(_template, index, Factory, Name, StartTick, Commands);
        }
    }
}