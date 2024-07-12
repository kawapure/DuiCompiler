using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mono.Options;

namespace Kawapure.DuiCompiler
{
    internal static class Options
    {
        public static bool s_noLogo { get; private set; } = false;

        public static string? s_inputFile { get; private set; } = null;

        public static string? s_outputFile { get; private set; } = null;

        public static bool s_showHelp { get; private set; } = false;

        public static List<string>? s_extraArgs { get; private set; } = null;

#if DEBUG
        public static bool s_debugParser = false;
#endif

        public static List<string> s_duiExts { get; private set; } = new() {
            ".dui", // Possible common extension.
            ".ui",  // Microsoft official extension
            ".uix", // Microsoft official extension
            ".xml", // Common in the modding community.
        };

        public static List<string> s_preprocessorExts { get; private set; } = new() {
            ".h",   // Standard C header file extension
            ".hpp", // C++-specific header file extension
            ".hxx", // C++-specific header file extension
            ".c",   // C source code file extension
            ".cpp", // C++ source code file extension
            ".cxx", // C++ source code file extension
        };

        public static OptionSet s_optionSet = new();

        public static bool ParseOptions(string[] args)
        {
            InitOptions();

            try
            {
                s_optionSet.Parse(args);
                return true;
            }
            catch (OptionException e)
            {
                Console.Error.WriteLine("Failed to parse options.");
                return false;
            }
        }

        private static void InitOptions()
        {
            s_optionSet
                .Add(
                    "nologo|nobanner",
                    "Don't display application information at startup.",
                    option =>
                    {
                        s_noLogo = true;
                    }
                )
                .Add("in|i=", "The input UI file", option =>
                    {
                        s_inputFile = option;
                    }
                )
                .Add("out|o=", "The output UI object (blank for stdout)", option =>
                    {
                        s_outputFile = option;
                    }
                )
                .Add(
                    "include-dirs|inc", 
                    "Comma-separated list of include directories to search for preprocessor " +
                    "header files in",
                    option =>
                    {

                    }
                )
                .Add("define|def=", "Comma-separated list of preprocessor defines", option =>
                    {

                    }
                )
                .Add(
                    "directui-exts|dui-exts=", 
                    "Comma-separated list of file extensions for DirectUI files. " +
                    "By default, this includes .dui, .ui, .uix, and .xml", 
                    option =>
                    {

                    }
                )
                .Add(
                    "preprocessor-exts|pp-exts=", 
                    "Comma-separated list of file extensions for the preprocessor. " +
                    "By default, this includes .h, .hpp, .hxx, .c, .cpp, and .cxx",
                    option =>
                    {

                    }
                )
                .Add("verbosity|verbose|v=", "Verbosity", option =>
                    {

                    }
                )
                .Add("version|ver", "Shows the version of the program", option =>
                    {

                    }
                )
#if DEBUG
                .Add(
                    "debug-parsing", 
                    "Print parse elements (tokens and parse nodes) into XML trees.",
                    option =>
                    {
                        s_debugParser = true;
                    }
                )
#endif
                .Add("help|h|?", "Shows help", option => s_showHelp = true);
        }

        public static void ShowHelp()
        {
            Console.WriteLine($"DuiCompiler DirectUI UI file compiler version {DuiCompilerMain.VERSION}");
            Console.WriteLine("by Isabella (kawapure)");
            Console.WriteLine();

#if DEBUG
            Console.WriteLine("DEBUG BUILD -- Debug commands available.");
            Console.WriteLine();
#endif

            Console.WriteLine("Basic usage: ");
            Console.WriteLine("    duic [--in <input file name>] [--out <output file name>]");
            Console.WriteLine();

            WrapTextForConsole(
                "If no output file name is passed, then output will be wrote to stdout."
            ).ForEach(line => Console.WriteLine(line));
            Console.WriteLine();

            Console.WriteLine("Options:");
            s_optionSet.WriteOptionDescriptions(Console.Out);
        }

        private static List<string> WrapTextForConsole(string text)
        {
            int windowWidth = Console.WindowWidth;

            // https://stackoverflow.com/a/29689349
            string[] words = text.Split(' ');
            List<string> lines = words.Skip(1).Aggregate(words.Take(1).ToList(), (l, w) =>
            {
                if (l.Last().Length + w.Length >= windowWidth)
                {
                    l.Add(w);
                }
                else
                {
                    l[l.Count - 1] += " " + w;
                }

                return l;
            });

            return lines;
        }
    }
}
