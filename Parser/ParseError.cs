using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kawapure.DuiCompiler.Parser
{
    internal class ParseError : Exception
    {
        protected SourceOrigin m_sourceOrigin;

        public ParseError(SourceOrigin sourceOrigin)
        {
            m_sourceOrigin = sourceOrigin;
        }

        public ParseError(Token token)
            : this(token.m_sourceOrigin)
        {
        }

        public ParseError(AParseNode parseNode)
            : this(parseNode.SourceOrigin)
        {
        }
    }
}
