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
    internal class WorldNode : ParseNode
    {
        public override string Name
        {
            get => "World";
        }

        public WorldNode(SourceOrigin sourceOrigin)
            : base(sourceOrigin)
        {}
    }
}
