using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kawapure.DuiCompiler.Parser
{
    internal class Token
    {
        public enum TokenType
        {
            /// <summary>
            /// Symbols are the majority of content in a source code file,
            /// including special characters like `#` and unquoted textual
            /// strings like `include`.
            /// </summary>
            SYMBOL,

            /// <summary>
            /// String literals are enclosed in quotes, and classify as entire
            /// tokens by themselves. An example is "resource.h".
            /// </summary>
            STRING_LITERAL,
        }

        public enum TokenLanguage
        {
            /// <summary>
            /// The token targets the DUIXML parser.
            /// </summary>
            DUIXML,

            /// <summary>
            /// The token targets the preprocessor parser.
            /// </summary>
            PREPROCESSOR,
        }

        public string m_string { get; protected set; }
        public readonly SourceOrigin m_sourceOrigin;
        public readonly TokenType m_type;
        public readonly TokenLanguage m_language;

        public Token(
            string token, 
            ITextReaderSourceProvider sourceProvider, 
            uint sourceFileOffset, 
            TokenType type = TokenType.SYMBOL,
            TokenLanguage language = TokenLanguage.DUIXML
        )
        {
            m_string = token;
            m_sourceOrigin = new SourceOrigin
            {
                sourceProvider = sourceProvider,
                cursorOffset = sourceFileOffset
            };
            m_type = type;
            m_language = language;
        }
    }
}
