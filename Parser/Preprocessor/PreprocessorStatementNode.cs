using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kawapure.DuiCompiler.Parser.Preprocessor
{
    internal class PreprocessorStatementNode : IParseNode
    {
        public string Name
        {
            get => "PreprocessorStatement";
        }

        public List<IParseNode> Children { get; } = new();
    }
}
