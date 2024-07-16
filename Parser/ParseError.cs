using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kawapure.DuiCompiler.Parser
{
    /// <summary>
    /// Represents a parse error.
    /// </summary>
    internal class ParseError : Exception
    {
        /// <summary>
        /// Message describing the nature of the error.
        /// </summary>
        protected string m_message;

        /// <summary>
        /// A reference to the source file.
        /// </summary>
        protected ITextReaderSourceProvider m_sourceProvider;

        /// <summary>
        /// Origin information of the content that caused this error.
        /// </summary>
        protected SourceOrigin m_sourceOrigin;

        public ParseError(string msg, ITextReaderSourceProvider sourceProvider)
        {
            m_message = msg;
            m_sourceProvider = sourceProvider;
        }

        public ParseError(string msg, SourceOrigin sourceOrigin)
        {
            m_message = msg;
            m_sourceProvider = sourceOrigin.sourceProvider;
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
