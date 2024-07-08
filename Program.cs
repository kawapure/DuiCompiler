using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Kawapure.DuiCompiler
{
    internal static class DuiCompilerMain
    {
        /// <summary>
        /// The version of the compiler.
        /// </summary>
        public static readonly string VERSION =
            Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString()
            ?? "unknown";

        /// <summary>
        /// The insertion point of the program.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            if (!Options.ParseOptions(args))
            {
                // Options should have already controlled the printing of any
                // error messages, so we'll just return here.
                return;
            }

            if (Options.s_showHelp)
            {
                Options.ShowHelp();
                return;
            }

            Console.WriteLine("Hello :3");

            if (Options.s_inputFile != null)
            {
                Parser.SourceFile fileReader = new(Options.s_inputFile, Parser.SourceFile.FileType.DUI_UIFILE);
                List<Parser.Token> tokens = fileReader.m_tokenizer.Tokenize();

                Console.WriteLine(tokens.Count);

                foreach (Parser.Token token in tokens)
                {
                    Console.WriteLine(token.m_string);
                }
            }
        }
    }
}