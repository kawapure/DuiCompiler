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
            Success,
            UnknownError,
            OutOfBounds,
        }

        readonly ITextReaderSourceProvider _source;
        int _cursor;

        public TextReader(ITextReaderSourceProvider fileReader, int defaultCursor = 0)
        {
            _source = fileReader;
            _cursor = defaultCursor;
        }

        public (Status, char) Peek(int offset = 1)
        {
            long fileOffset = _cursor + offset;

            char nextCharacter;

            if (fileOffset <= _source.Contents.Length)
            {
                try
                {
                    nextCharacter = _source.Contents[(int)fileOffset];
                }
                catch (Exception)
                {
                    return (Status.UnknownError, char.MinValue);
                }
            }
            else
            {
                return (Status.OutOfBounds, char.MinValue);
            }

            return (Status.Success, nextCharacter);
        }

        public (Status, char) Read(int offset = 0)
        {
            (Status status, char character) = Peek(offset);

            _cursor += offset + 1;

            return (status, character);
        }

        public (Status, char) Rewind(int offset = 1)
        {
            (Status s, char c) = Peek(-1 * offset);

            _cursor -= offset;

            return (s, c);
        }

        public bool LookAheadForChar(char nextChar, int offset = 1)
        {
            try
            {
                return _source.Contents[_cursor + offset] == nextChar;
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
                return _source.Contents.Substring(_cursor + offset, sequence.Length) == sequence;
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
            _cursor = 0;
        }

        public Status SetCursor(int position)
        {
            if (position > _source.Contents.Length)
            {
                return Status.OutOfBounds;
            }

            _cursor = position;

            return Status.Success;
        }

        public int GetCurrentOffset()
        {
            return _cursor;
        }
    }
}
