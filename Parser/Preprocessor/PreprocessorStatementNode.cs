using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kawapure.DuiCompiler.Parser.Preprocessor
{
    internal class PreprocessorStatementNode : AParseNode
    {
        private readonly string m_name = "PreprocessorStatement";

        public override string Name
        {
            get => m_name;
            protected set { }
        }

        public PreprocessorStatementNode(SourceOrigin sourceOrigin)
            : base(sourceOrigin)
        {
        }
    }
}
