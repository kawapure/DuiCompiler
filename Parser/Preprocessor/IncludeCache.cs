using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kawapure.DuiCompiler.Parser.Preprocessor
{
    internal class IncludeCache
    {
        public Dictionary<string, IIncludeCacheItem> FilePaths { get; protected set; } = new();
    }
}
