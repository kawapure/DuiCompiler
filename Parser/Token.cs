using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kawapure.DuiCompiler.Parser
{
    internal class Token
    {
        [Flags]
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

        public string m_string { get; protected set; }
        public readonly SourceOrigin m_sourceOrigin;
        public readonly TokenType m_type;

        public Token(string token, SourceFile sourceFileReader, uint sourceFileOffset, TokenType type = TokenType.SYMBOL)
        {
            m_string = token;
            m_sourceOrigin = new SourceOrigin
            {
                sourceFileReader = sourceFileReader,
                cursorOffset = sourceFileOffset
            };
            m_type = type;
        }
    }
}
