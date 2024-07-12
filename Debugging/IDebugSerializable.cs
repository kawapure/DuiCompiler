using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Kawapure.DuiCompiler.Debugging
{
    internal interface IDebugSerializable
    {
#if DEBUG
        internal XElement DebugSerialize();
#endif
    }
}
