using System;
using System.Windows.Forms;

namespace TiddlyWikiWatcher
{
    static class Program
    {
        static string _filename = null;
        static bool _autoOpen = false;

        static void ParseCommandLine(string[] args)
        {
            string arg;
            for (int i = 0; i < args.Length; i++)
            {
                arg = args[i];
                if (arg.Length >= 9 && arg.Substring(0, 9).ToLower() == "-autoopen")
                {
                    _autoOpen = true;
                }
                else if (arg.Length > 0)
                {
                    _filename = arg;
                }
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            ParseCommandLine(args);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(new TiddlyWikiWatcher(), _filename, _autoOpen));
        }
    }
}
