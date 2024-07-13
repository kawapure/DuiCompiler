using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kawapure.DuiCompiler.Parser.Preprocessor
{
    // TODO: remove, replaced by ParseNode
    internal class PreprocessorStatementNode : ParseNode
    {
        private readonly string m_name = "PreprocessorStatement";

        public override string Name
        {
            get => m_name;
        }

        public PreprocessorStatementNode(string name, SourceOrigin sourceOrigin)
            : base(sourceOrigin)
        {
            m_name = name;
        }
    }
}
