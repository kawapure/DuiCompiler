using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if DEBUG
using Kawapure.DuiCompiler.Debugging;
using System.Xml.Linq;
#endif

namespace Kawapure.DuiCompiler.Parser
{
    internal struct TokenStream
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

#if DEBUG
        public XElement DebugSerialize()
        {
            XElement result = new("TokenStream");

            foreach (Token token in this.tokens)
            {
                result.Add(token.DebugSerialize());
            }

            return result;
        }
#endif
    }
}
