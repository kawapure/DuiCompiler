using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kawapure.DuiCompiler.Parser
{
    internal class TextReader
    {
        public enum Status
        {
            SUCCESS,
            UNKNOWN_ERROR,
            OUT_OF_BOUNDS,
        }

        readonly ITextReaderSourceProvider m_source;
        int m_cursor;

        public TextReader(ITextReaderSourceProvider fileReader, int defaultCursor = 0)
        {
            m_source = fileReader;
            m_cursor = defaultCursor;
        }

        public (Status, char) Peek(int offset = 1)
        {
            long fileOffset = m_cursor + offset;

            char nextCharacter;

            if (fileOffset <= m_source.Contents.Length)
            {
                try
                {
                    nextCharacter = m_source.Contents[(int)fileOffset];
                }
                catch (Exception)
                {
                    return (Status.UNKNOWN_ERROR, char.MinValue);
                }
            }
            else
            {
                return (Status.OUT_OF_BOUNDS, char.MinValue);
            }

            return (Status.SUCCESS, nextCharacter);
        }

        public (Status, char) Read(int offset = 0)
        {
            (Status status, char character) = Peek(offset);

            m_cursor += offset + 1;

            return (status, character);
        }

        public (Status, char) Rewind(int offset = 1)
        {
            (Status s, char c) = Peek(-1 * offset);

            m_cursor -= offset;

            return (s, c);
        }

        public bool LookAheadForChar(char nextChar, int offset = 1)
        {
            try
            {
                return m_source.Contents[m_cursor + offset] == nextChar;
            }
            catch
            {
                return false;
            }
        }

        public bool LookBehindForChar(char prevChar, int offset = 0)
        {
            return LookAheadForChar(prevChar, -1 + offset);
        }

        public bool LookAheadForSequence(string sequence, int offset = 1)
        {
            try
            {
                return m_source.Contents.Substring(m_cursor + offset, sequence.Length) == sequence;
            }
            catch
            {
                return false;
            }
        }

        public bool LookBehindForSequence(string sequence, int offset = 0)
        {
            return LookAheadForSequence(sequence, (-1 * sequence.Length) + offset);
        }

        //public (Status, char) Current()
        //{
        //    return Peek(0);
        //}

        public void Reset()
        {
            m_cursor = 0;
        }

        public Status SetCursor(int position)
        {
            if (position > m_source.Contents.Length)
            {
                return Status.OUT_OF_BOUNDS;
            }

            m_cursor = position;

            return Status.SUCCESS;
        }

        public int GetCurrentOffset()
        {
            return m_cursor;
        }
    }
}
