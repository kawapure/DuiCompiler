using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kawapure.DuiCompiler.Parser
{
    /// <summary>
    /// The top-level node of the parser environment.
    /// </summary>
    internal class WorldNode : IParseNode
    {
        public string Name
        {
            get => "World";
        }

        public List<IParseNode> Children { get; } = new();
    }
}
