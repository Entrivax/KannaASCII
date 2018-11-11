namespace KannaASCII
{
    class CommandLineArguments
    {
        [Argument("help", 'h', "Display the help")]
        public bool Help { get; set; }

        [Argument("animation", 'a', "Select a path for the animation to play")]
        public string AnimationFile { get; set; }

        [Argument("time-before-close", 't', "Set the time in seconds of showing the result after the animation ends playing")]
        public float? TimeBeforeClose { get; set; }
    }
}
