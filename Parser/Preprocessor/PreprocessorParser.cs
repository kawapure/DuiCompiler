using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kawapure.DuiCompiler.Parser.Preprocessor
{
    /// <summary>
    /// Parses preprocessor token lists.
    /// </summary>
    internal class PreprocessorParser
    {
        public enum KeywordState
        {
            SUPPORTED,
            UNSUPPORTED,
            PREPROCESSOR_ONLY,
            INVALID,
        }

        protected readonly IncludeCache m_pragmaOnceStore;
        protected readonly SourceFile m_sourceFile;
        protected WorldNode m_world = new();

        public PreprocessorParser(SourceFile sourceFile)
        {
            m_sourceFile = sourceFile;
        }

        public IParseNode ParseTokenSequence(List<Token> tokens)
        {
            // TODO:
            return new PreprocessorStatementNode();
        }

        public KeywordState QueryKeywordState(string keyword)
        {
            return keyword switch
            {
                "if"      => KeywordState.SUPPORTED,
                "ifdef"   => KeywordState.SUPPORTED,
                "ifndef"  => KeywordState.SUPPORTED,
                "elif"    => KeywordState.SUPPORTED,
                "else"    => KeywordState.SUPPORTED,
                "endif"   => KeywordState.SUPPORTED,
                "define"  => KeywordState.SUPPORTED,
                "undef"   => KeywordState.SUPPORTED,
                "include" => KeywordState.SUPPORTED,
                "pragma"  => KeywordState.SUPPORTED,
                "error"   => KeywordState.SUPPORTED,
                 
                "import"  => KeywordState.UNSUPPORTED,
                "line"    => KeywordState.UNSUPPORTED,
                "using"   => KeywordState.UNSUPPORTED,

                _ => KeywordState.INVALID
            };
        }
    }
}
