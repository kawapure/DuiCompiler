﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kawapure.DuiCompiler.Parser
{
    internal interface ITextReaderSourceProvider
    {
        public string Contents { get; }
        public TextReader GetNewReader(int defaultOffset = 0);
        public int[] GetLineOffsets();
    }
}
