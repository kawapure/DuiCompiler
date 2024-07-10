using System;
using System.Reflection;

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

            // Since we write the source code to stdout, we want to write other
            // messages to stderr so they don't aren't intertwined with the
            // result.
            if (!Options.s_noLogo)
            {
                Console.Error.WriteLine("Hello :3");
            }

            if (Options.s_inputFile != null)
            {
                Parser.SourceFile fileReader = new(Options.s_inputFile, Parser.SourceFile.FileType.DUI_UIFILE);
                List<Parser.Token> tokens = fileReader.m_tokenizer.Tokenize();

                Console.Error.WriteLine(tokens.Count);

                foreach (Parser.Token token in tokens)
                {
                    Console.WriteLine(token.m_string);
                }
            }
        }
    }
}