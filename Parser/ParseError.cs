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
        protected string _message;

        /// <summary>
        /// A reference to the source file.
        /// </summary>
        protected ITextReaderSourceProvider _sourceProvider;

        /// <summary>
        /// Origin information of the content that caused this error.
        /// </summary>
        protected SourceOrigin _sourceOrigin;

        public ParseError(string msg, ITextReaderSourceProvider sourceProvider)
        {
            _message = msg;
            _sourceProvider = sourceProvider;
        }

        public ParseError(string msg, SourceOrigin sourceOrigin)
        {
            _message = msg;
            _sourceProvider = sourceOrigin.sourceProvider;
            _sourceOrigin = sourceOrigin;
        }

        public ParseError(string msg, Token token)
            : this(msg, token._sourceOrigin)
        {
        }

        public ParseError(string msg, ParseNode parseNode)
            : this(msg, parseNode.SourceOrigin)
        {
        }
    }
}
