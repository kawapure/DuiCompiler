using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kawapure.DuiCompiler.Parser
{
    internal class ParseError : Exception
    {
        protected string m_message;
        protected ITextReaderSourceProvider? m_sourceProvider;
        protected SourceOrigin? m_sourceOrigin;

        public ParseError(string msg, ITextReaderSourceProvider sourceProvider)
        {
            m_message = msg;
            m_sourceProvider = sourceProvider;
        }

        public ParseError(string msg, SourceOrigin sourceOrigin)
        {
            m_message = msg;
            m_sourceOrigin = sourceOrigin;
        }

        public ParseError(string msg, Token token)
            : this(msg, token.m_sourceOrigin)
        {
        }

        public ParseError(string msg, ParseNode parseNode)
            : this(msg, parseNode.SourceOrigin)
        {
        }
    }
}
