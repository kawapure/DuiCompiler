using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Kawapure.DuiCompiler.Parser
{
    internal struct SourceOrigin
    {
        public ITextReaderSourceProvider sourceProvider;
        public uint cursorOffset;

        /// <summary>
        /// Get the line number of the current cursor offset.
        /// </summary>
        public int GetLine()
        {
            int[] lineOffsets = sourceProvider.GetLineOffsets();

            for (int i = lineOffsets.Length - 1; i > 0; i--)
            {
                if (lineOffsets[i] > this.cursorOffset)
                {
                    continue;
                }

                // Arrays are zero-indexed; lines are one-indexed.
                return i + 1;
            }

            return 1;
        }

        /// <summary>
        /// Get the column number relative to the line of the current cursor
        /// offset.
        /// </summary>
        public int GetLineColumn()
        {
            int[] lineOffsets = sourceProvider.GetLineOffsets();

            for (int i = lineOffsets.Length - 1; i >= 0; i--)
            {
                if (lineOffsets[i] >= this.cursorOffset)
                {
                    continue;
                }

                int result = lineOffsets[i] - (int)this.cursorOffset;

                if (this.cursorOffset == 1)
                {
                    Debug.Assert(result == -1);
                }

                // Arrays are zero-indexed; columns are one-indexed.
                return (-1 * result) + 1;
            }

            return 1;
        }
    }
}
