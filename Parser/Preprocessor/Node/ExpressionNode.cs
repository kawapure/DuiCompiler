﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kawapure.DuiCompiler.Parser.Preprocessor.Node
{
    internal class ExpressionNode : ParseNode
    {
        public override string Name
        {
            get => "Expression";
        }

        public ExpressionNode(SourceOrigin sourceOrigin)
            : base(sourceOrigin)
        {}
    }
}
