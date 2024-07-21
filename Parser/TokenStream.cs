using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

#if DEBUG
using Kawapure.DuiCompiler.Debugging;
using System.Xml.Linq;
#endif

namespace Kawapure.DuiCompiler.Parser
{
    internal class TokenStream
#if DEBUG
        : IDebugSerializable
#endif
    {
        public readonly List<Token> tokens;
        public int position = 0;

        public TokenStream(List<Token> tokenList)
        {
            this.tokens = tokenList;
        }

        public Token this[int i]
        {
            get => tokens[i + this.position];
        }

        public Token Next()
        {
            if (tokens.Count > this.position - 1)
            {
                SourceOrigin lastSrcOrigin = tokens[this.position - 1].m_sourceOrigin;

                throw new ParseError(
                    "Unexpected end of file",
                    lastSrcOrigin
                );
            }

            return this[this.position++];
        }

        public Token Expect(string nextSequence, string? errorMsg = null)
        {
            Token token = this[this.position];

            if (token.m_string != nextSequence)
            {
                throw new ParseError(
                    $"Unexpected token \"{token.ToSafeString}\", " +
                    $"expected \"{Token.ToSafeString(nextSequence)}\"" +
                    null != errorMsg ? $" {errorMsg}" : "",
                    token.m_sourceOrigin
                );
            }

            return token;
        }
        
        public Token ExpectNext(string nextSequence, string? errorMsg = null)
        {
            this.position++;
            return Expect(nextSequence, errorMsg);
        }

#if DEBUG
        public XElement DebugSerialize()
        {
            XElement result = new("TokenStream");
            result.SetAttributeValue("ChildElementCount", this.tokens.Count);

            foreach (Token token in this.tokens)
            {
                result.Add(token.DebugSerialize());
            }

            return result;
        }
#endif
    }
}
