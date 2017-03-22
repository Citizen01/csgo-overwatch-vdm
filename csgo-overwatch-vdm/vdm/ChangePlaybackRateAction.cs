namespace csgo_overwatch_vdm.vdm
{
    internal class ChangePlaybackRateAction : DemoActionBase
    {
        public int StopTick { get; set; }
        public int PlaybackRate { get; set; }

        public ChangePlaybackRateAction()
        {
            Factory = Name = "ChangePlaybackRate";
        }

        private static string _template = @"
    ""{0}""
    {{
        factory ""{1}""
        name ""{2}""
        starttick ""{3}""
        stoptick ""{4}""
        playbackrate ""{5}""
    }}";

        public override string ToString(int index)
        {
            return string.Format(_template, index, Factory, Name, StartTick, StopTick, PlaybackRate);
        }
    }
}