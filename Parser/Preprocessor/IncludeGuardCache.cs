using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kawapure.DuiCompiler.Parser.Preprocessor
{
    internal class IncludeGuardCache : IIncludeCacheItem
    {
        // SourceFile is a PLACEHOLDER class!!!!!!!!
        // It should be replaced with the object for a preprocessor define
        // when the time comes for that to be implemented.
        protected WeakReference<SourceFile> m_skipTarget;

        public IncludeGuardCache(SourceFile a)
        {
            m_skipTarget = new(a);
        }

        public bool ShouldSkip()
        {
            if (m_skipTarget.TryGetTarget(out SourceFile? a))
            {
                if (a != null)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
