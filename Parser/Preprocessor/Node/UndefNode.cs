using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kawapure.DuiCompiler.Parser.Preprocessor.Node
{
    internal class UndefNode : ParseNode
    {
        public override string Name
        {
            get => "Undef";
        }

        public UndefNode(SourceOrigin sourceOrigin)
            : base(sourceOrigin)
        {
            this.Expression = new(sourceOrigin);
            AppendChild(this.Expression);
        }

        public UndefNode(SourceOrigin sourceOrigin, string value)
            : this(sourceOrigin)
        {
            this.Value = value;
        }

        public ExpressionNode Expression { get; protected set; }

        //---------------------------------------------------------------------

        public string Value
        {
            get => GetAttribute("Value") ?? throw new Exception("Empty-valued undef statement.");
            set => SetAttribute("Value", value);
        }
    }
}
