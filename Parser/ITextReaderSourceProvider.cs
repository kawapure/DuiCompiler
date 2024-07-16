using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kawapure.DuiCompiler.Parser
{
    /// <summary>
    /// Provides a wrapper for source code text files or file-like objects.
    /// </summary>
    internal interface ITextReaderSourceProvider
    {
        /// <summary>
        /// Textual content of the source code file.
        /// </summary>
        public string Contents { get; }

        /// <summary>
        /// Gets a new TextReader at the specified offset, or at the beginning
        /// of the file if none is specified.
        /// </summary>
        public TextReader GetNewReader(int defaultOffset = 0);

        /// <summary>
        /// Gets an array of all byte offsets of new line characters in the
        /// source text.
        /// </summary>
        public int[] GetLineOffsets();
    }
}
