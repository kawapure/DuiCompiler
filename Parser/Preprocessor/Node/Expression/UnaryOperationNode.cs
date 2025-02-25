using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Kawapure.DuiCompiler.Parser.Preprocessor.Node.Expression
{
    internal class UnaryOperationNode : ParseNode, IExpressionOperation
    {
        public override string Name
        {
            get => "UnaryOperation";
        }

        public UnaryOperationNode(EType type, SourceOrigin sourceOrigin)
            : base(sourceOrigin)
        {
            this.Type = type;
        }

        public override bool Validate()
        {
            if (base.Validate())
            {
                // We only want one child on a unary operation node, but this
                // shouldn't ever be deviated from except in developer builds,
                // so only a debug assert is used.
                Debug.Assert(this.Children.Count == 1);

                if (this.Operand is ParseNode)
                {
                    return true;
                }
            }

            return false;
        }

        //---------------------------------------------------------------------

        public enum EType
        {
            Invalid,
            BitwiseNot,
            LogicalNot,
        }

#pragma warning disable CS8604 // Possible null reference argument for parameter 'value'

        public EType Type
        {
            get => (EType)Enum.Parse(typeof(EType), GetAttribute("Type") ?? "INVALID");
            set => SetAttribute("Type", Enum.GetName(typeof(EType), value));
        }

        private ParseNode m_operand;
        public ParseNode? Operand
        {
            get => m_operand;
            set
            {
                // If we already have a child, then we want to throw out that
                // child since a unary expression may only have one single
                // operand at a time.
                if (this.Children.Count > 0)
                {
                    foreach (ParseNode child in this.Children)
                    {
                        child.RemoveParentRelationship();
                    }
                }

                this.Children.Add(value);
            }
        }

#pragma warning restore
    }
}
