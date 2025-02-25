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
            Symbol,

            /// <summary>
            /// String literals are enclosed in quotes, and classify as entire
            /// tokens by themselves. An example is "resource.h".
            /// </summary>
            StringLiteral,
        }

        public enum TokenLanguage
        {
            /// <summary>
            /// The token targets the DUIXML parser.
            /// </summary>
            DuiXml,

            /// <summary>
            /// The token targets the preprocessor parser.
            /// </summary>
            Preprocessor,
        }

        public string _string { get; protected set; }
        public readonly SourceOrigin _sourceOrigin;
        public readonly TokenType _type;
        public readonly TokenLanguage _language;

        public Token(
            string token, 
            ITextReaderSourceProvider sourceProvider, 
            uint sourceFileOffset, 
            TokenType type = TokenType.Symbol,
            TokenLanguage language = TokenLanguage.DuiXml
        )
        {
            _string = token;
            _sourceOrigin = new SourceOrigin
            {
                sourceProvider = sourceProvider,
                cursorOffset = sourceFileOffset
            };
            _type = type;
            _language = language;
        }

        public override string ToString()
        {
            return _string;
        }

        public static explicit operator string(Token token)
        {
            return token.ToString();
        }

        public string ToSafeString()
        {
            return Token.ToSafeString(_string);
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

            if (_sourceOrigin.sourceProvider is not SourceFile)
            {
                result.SetAttributeValue("AnonymousSource", "true");
            }
            else
            {
                SourceFile sourceFile = (SourceFile)_sourceOrigin.sourceProvider;

                result.SetAttributeValue(
                    "SourceFile",
                    sourceFile.Path
                );
            }

            result.SetAttributeValue(
                "SourceOffset",
                _sourceOrigin.cursorOffset
            );

            result.SetAttributeValue(
                "SourceLineColumn",
                _sourceOrigin.GetLine() + ":" + _sourceOrigin.GetLineColumn()
            );

            result.SetAttributeValue(
                "TokenType",
                Enum.GetName(typeof(TokenType), _type)
            );

            result.SetAttributeValue(
                "TokenLanguage",
                Enum.GetName(typeof(TokenLanguage), _language)
            );

            if (_string == "\x00")
            {
                result.SetAttributeValue("SpecialValue", "Null");
            }
            else if (_string == "\n")
            {
                result.SetAttributeValue("SpecialValue", "NewLine");
            }
            else
            {
                XText text = new(_string);
                result.Add(text);
            }

            return result;
        }
#endif
    }
}
