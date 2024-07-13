using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kawapure.DuiCompiler.Parser.Preprocessor.Node
{
    internal class IfNode : ParseNode
    {
        public override string Name
        {
            get => "If";
        }

        public IfNode(SourceOrigin sourceOrigin)
            : base(sourceOrigin)
        {
            this.Expression = new(sourceOrigin);
            AppendChild(this.Expression);
        }

        public ExpressionNode Expression { get; protected set; }

        //---------------------------------------------------------------------

        public ParseNode AddElse(SourceOrigin sourceOrigin)
        {
            ElseNode elseNode = new(sourceOrigin);

            AppendChild(elseNode);

            return elseNode;
        }
    }
}
