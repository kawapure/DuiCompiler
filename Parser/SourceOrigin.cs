﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kawapure.DuiCompiler.Parser
{
    internal struct SourceOrigin
    {
        public SourceFile sourceFileReader;
        public uint cursorOffset;
    }
}
