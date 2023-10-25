using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace TiddlyWikiWatcher
{
    static class Program
    {
        static string _filename = null;
        static bool _autoOpen = false;

        public static List<string> installationFilenames = new List<string>()
        {
                "TiddlyWikiWatcher.exe",
                "TiddlyWikiWatcher.exe.config",

                "Microsoft.Web.WebView2.Core.dll",
                "Microsoft.Web.WebView2.WinForms.dll",
                "Microsoft.Web.WebView2.Wpf.dll",

#if WIN32
                "runtimes\\win-x86\\native\\WebView2Loader.dll",
#elif WIN64
                "runtimes\\win-x64\\native\\WebView2Loader.dll",
#else
                #error WIN32 or WIN64 must be defined
#endif

        };

        private static void OutputVersion(string outputFilename)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            File.WriteAllText(outputFilename, version.Major + "." + version.Minor, Encoding.ASCII);
        }

        private static void OutputInstallationFilenames(string outputFilename)
        {
            StringBuilder output = new StringBuilder();
            foreach (var filename in installationFilenames)
            {
                output.AppendLine(filename);
            }

            File.WriteAllText(outputFilename, output.ToString(), Encoding.ASCII);
        }

        private static bool handleVersionCommand(string[] args)
        {
            if (args.Length == 1 &&
                args[0].Length > 10 && args[0].Substring(0, 10).ToLowerInvariant() == "--version=")
            {
                // "--version=d:\Projects\TiddlyWikiWatcher\assets\Release_version.txt"
                var filename = args[0].Substring(10).Trim();
                if (!string.IsNullOrWhiteSpace(filename) && filename.IndexOfAny(Path.GetInvalidPathChars()) < 0)
                {
                    OutputVersion(filename);
                }
                return true;
            }

            return false;
        }

        private static bool handleInstallationFilesCommand(string[] args)
        {
            if (args.Length == 1 &&
                args[0].Length > 24 && args[0].Substring(0, 24).ToLowerInvariant() == "--installationfilenames=")
            {
                // "--installationFilenames=d:\Projects\TiddlyWikiWatcher\assets\Release_filenames.txt"
                var filename = args[0].Substring(24).Trim();
                if (!string.IsNullOrWhiteSpace(filename) && filename.IndexOfAny(Path.GetInvalidPathChars()) < 0)
                {
                    OutputInstallationFilenames(filename);
                }
                return true;
            }

            return false;
        }

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
            if (handleVersionCommand(args)) return;
            if (handleInstallationFilesCommand(args)) return;

            ParseCommandLine(args);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(_filename, _autoOpen));
        }
    }
}
