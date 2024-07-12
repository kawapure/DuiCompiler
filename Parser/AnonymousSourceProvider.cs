using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kawapure.DuiCompiler.Parser
{
    internal class AnonymousSourceProvider : ITextReaderSourceProvider
    {
        public string Contents { get; protected set; }

        public AnonymousSourceProvider(string contents)
        {
            this.Contents = contents;
        }

        /// <summary>
        /// Gets a new reader. If you're reading the file
        /// asynchronously (i.e. multithreaded environment), then you'll want
        /// to use this instead of the shared context.
        /// </summary>
        public TextReader GetNewReader(int defaultOffset = 0)
        {
            return new TextReader(this, defaultOffset);
        }
    }
}
