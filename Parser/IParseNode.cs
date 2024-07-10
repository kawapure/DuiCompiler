using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kawapure.DuiCompiler.Parser
{
    internal interface IParseNode
    {
        public string Name { get; }
        public List<IParseNode> Children { get; }
    }
}
