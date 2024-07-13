using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

#if DEBUG
using Kawapure.DuiCompiler.Debugging;
using System.Xml.Linq;
#endif

namespace Kawapure.DuiCompiler.Parser
{
    internal class Token
#if DEBUG
        : IDebugSerializable
#endif
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

        public override string ToString()
        {
            return m_string;
        }

        public static explicit operator string(Token token)
        {
            return token.ToString();
        }

        public string ToSafeString()
        {
            return Token.ToSafeString(m_string);
        }

        public static string ToSafeString(string token)
        {
            if (token == "\0")
            {
                return "[NUL]";
            }
            else if (token == "\n")
            {
                return "\\n";
            }
            
            return token;
        }


#if DEBUG
        public XElement DebugSerialize()
        {
            XElement result = new("Token");

            result.SetAttributeValue("NativeClassName", this.GetType().Name);

            if (m_sourceOrigin.sourceProvider is not SourceFile)
            {
                result.SetAttributeValue("AnonymousSource", "true");
            }
            else
            {
                SourceFile sourceFile = (SourceFile)m_sourceOrigin.sourceProvider;

                result.SetAttributeValue(
                    "SourceFile",
                    sourceFile.Path
                );
            }

            result.SetAttributeValue(
                "SourceOffset",
                m_sourceOrigin.cursorOffset
            );

            result.SetAttributeValue(
                "TokenType",
                Enum.GetName(typeof(TokenType), m_type)
            );

            result.SetAttributeValue(
                "TokenLanguage",
                Enum.GetName(typeof(TokenLanguage), m_language)
            );

            if (m_string == "\x00")
            {
                result.SetAttributeValue("SpecialValue", "Null");
            }
            else if (m_string == "\n")
            {
                result.SetAttributeValue("SpecialValue", "NewLine");
            }
            else
            {
                XText text = new(m_string);
                result.Add(text);
            }

            return result;
        }
#endif
    }
}
