using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kawapure.DuiCompiler.Parser.Preprocessor.Node.Expression
{
    internal class BinaryOperationNode : ParseNode, IExpressionOperation
    {
        public override string Name
        {
            get => "BinaryOperation";
        }

        public BinaryOperationNode(EType type, SourceOrigin sourceOrigin)
            : base(sourceOrigin)
        {
            this.Type = type;

            m_leftOperand = new("LeftOperand", sourceOrigin);
            m_rightOperand = new("RightOperand", sourceOrigin);
        }

        public override bool Validate()
        {
            if (base.Validate())
            {
                // We only want exactly two children on a binary operation node,
                // but this shouldn't ever be deviated from except in developer
                // builds, so only a debug assert is used.
                Debug.Assert(this.Children.Count == 2);
                Debug.Assert(m_leftOperand.Children.Count == 1);
                Debug.Assert(m_rightOperand.Children.Count == 1);
            }

            return false;
        }

        //---------------------------------------------------------------------

        public enum EType
        {
            INVALID,
            BITWISE_AND,
            BITWISE_OR,
            BITWISE_XOR,
            BITWISE_LSHIFT,
            BITWISE_RSHIFT,
            LOGICAL_AND,
            LOGICAL_OR,
        }

#pragma warning disable CS8604 // Possible null reference argument for parameter 'value'

        public EType Type
        {
            get => (EType)Enum.Parse(typeof(EType), GetAttribute("Type") ?? "INVALID");
            set => SetAttribute("Type", Enum.GetName(typeof(EType), value));
        }

        protected ParseNode m_leftOperand;
        protected ParseNode m_rightOperand;

        public ParseNode? LeftOperand
        {
            get => m_leftOperand.Children[0] ?? null;
            set
            {
                RemoveOperandChildrenIfNecessary(m_leftOperand);
                m_leftOperand.Children.Add(value);
            }
        }

        public ParseNode? RightOperand
        {
            get => m_leftOperand.Children[0] ?? null;
            set
            {
                RemoveOperandChildrenIfNecessary(m_rightOperand);
                m_rightOperand.Children.Add(value);
            }
        }

        protected void RemoveOperandChildrenIfNecessary(ParseNode operand)
        {
            if (operand.Children.Count > 0)
            {
                foreach (ParseNode child in operand.Children)
                {
                    child.RemoveParentRelationship();
                }
            }
        }

#pragma warning restore
    }
}
