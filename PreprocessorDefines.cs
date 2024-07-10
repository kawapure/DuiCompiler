using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kawapure.DuiCompiler
{
    internal class PreprocessorDefines
    {
        protected static Dictionary<string, string> g_global = new();

        public static Dictionary<string, string> GetGlobal()
        {
            return g_global;
        }
    }
}
