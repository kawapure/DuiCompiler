using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kawapure.DuiCompiler.Parser
{
    /// <summary>
    /// Responsible for calculating line offsets and caching them.
    /// </summary>
    internal class LineOffsetManager
    {
        protected string _source;

        protected int[]? _cachedOffsets = null;

        public LineOffsetManager(string source)
        {
            _source = source;
        }

        /// <summary>
        /// Gets an array of all byte offsets of new line characters in the
        /// source text.
        /// </summary>
        public int[] GetLineOffsets()
        {
            /*
             * TODO: Account offsets for tab count provided by user (param).
             * 
             * Tab characters take up a variable, user-defined number of white
             * space columns (usually 4). Since they are simply one byte, they
             * complicate things a bit.
             * 
             * Thus, the current algorithm gets the wrong offset for the user's
             * text editor if tabs are used instead of spaces.
             */

            if (_cachedOffsets is not null)
            {
                return _cachedOffsets;
            }
            else
            {
                List<int> offsets = new();

                for (int i = 0; i < _source.Length && i != -1; i = _source.IndexOf("\n", i + 1))
                {
                    offsets.Add(i);
                }

                int[] arr = offsets.ToArray();
                _cachedOffsets = arr;
                return _cachedOffsets;
            }
        }
    }
}
