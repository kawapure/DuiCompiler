﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kawapure.DuiCompiler.Parser.Preprocessor.Node
{
    internal class ElseNode : ParseNode
    {
        public override string Name
        {
            get => "Else";
        }

        public ElseNode(SourceOrigin sourceOrigin)
            : base(sourceOrigin)
        {}
    }
}
