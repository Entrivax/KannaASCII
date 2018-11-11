using System;
using System.Windows.Forms;

namespace KannaASCII
{
    static class Program
    {
        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            var arguments = CommandLineHelper.Parse<CommandLineArguments>(args);
            if (arguments.Help)
            {
                CommandLineHelper.WriteHelp<CommandLineArguments>();
                return;
            }

            var animationFile = "animation.json";
            if (arguments.AnimationFile != null)
            {
                animationFile = arguments.AnimationFile;
            }

            var timeBeforeClose = 5f;
            if (arguments.TimeBeforeClose.HasValue)
            {
                timeBeforeClose = arguments.TimeBeforeClose.Value;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(animationFile, timeBeforeClose));
        }
    }
}
