using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Kawapure.DuiCompiler.Debugging
{
    /// <summary>
    /// A debug-only interface denoting debugging capabilities of the current
    /// object. Wrap implementations in #if DEBUG preprocessor conditions.
    /// </summary>
    internal interface IDebugSerializable
    {
#if DEBUG
        /// <summary>
        /// 
        /// Gets a serialized version of the content for debugging purposes.
        /// 
        /// </summary>
        /// 
        /// <returns> An XML element representing the implementing object. </returns>
        internal XElement DebugSerialize();
#endif
    }
}
