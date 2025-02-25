using Kawapure.DuiCompiler.Parser;
using System;
using System.Reflection;

#if DEBUG
using Kawapure.DuiCompiler.Debugging;
using System.Xml;
using System.Xml.Linq;
#endif

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

            // Since we write the resulting code to stdout, we want to write other
            // messages to stderr so they don't aren't intertwined with the
            // result.
            if (!Options.s_noLogo)
            {
                Console.Error.WriteLine("Hello :3");
            }

            if (Options.s_inputFile != null)
            {
                Parser.SourceFile fileReader = new(Options.s_inputFile, Parser.SourceFile.FileType.DuiUiFile);
                List<Parser.Token> tokens = fileReader._tokenizer.Tokenize();

                TokenStream tokenStream = new(tokens);

                Console.Error.WriteLine(tokens.Count);

#if DEBUG
                if (Options.s_debugParser)
                {
                    // Print the XML stream to stdout:
                    Console.Error.WriteLine("Debug XML will be printed to stdout...");

                    XElement debugTree = tokenStream.DebugSerialize();
                    Console.Out.Write(debugTree.ToString());

                    return;
                }
#endif
            }
        }
    }
}