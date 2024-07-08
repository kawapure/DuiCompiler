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
        public static string? s_inputFile { get; private set; } = null;

        public static bool s_showHelp { get; private set; } = false;

        public static List<string>? s_extraArgs { get; private set; } = null;

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
                return false;
            }
        }

        private static void InitOptions()
        {
            s_optionSet
                .Add("in|i=", "The input UI file", option =>
                {
                    s_inputFile = option;
                })
                .Add("out|o=", "The output UI object (blank for stdout)", option =>
                {

                })
                .Add("include-dirs", "Comma-separated list of include directories to search for preprocessor header files in", option =>
                {

                })
                .Add("define|def=", "Comma-separated list of preprocessor defines", option =>
                {

                })
                .Add("help|h|?", "Shows help", option => s_showHelp = true);
        }

        public static void ShowHelp()
        {
            Console.WriteLine($"DuiCompiler DirectUI UI file compiler version {DuiCompilerMain.VERSION}");
            Console.WriteLine("by Isabella (kawapure)");
            Console.WriteLine();

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
