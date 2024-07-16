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
        protected string m_source;

        protected int[]? m_cachedOffsets = null;

        public LineOffsetManager(string source)
        {
            m_source = source;
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

            if (m_cachedOffsets is not null)
            {
                return m_cachedOffsets;
            }
            else
            {
                List<int> offsets = new();

                for (int i = 0; i < m_source.Length && i != -1; i = m_source.IndexOf("\n", i + 1))
                {
                    offsets.Add(i);
                }

                int[] arr = offsets.ToArray();
                m_cachedOffsets = arr;
                return m_cachedOffsets;
            }
        }
    }
}
