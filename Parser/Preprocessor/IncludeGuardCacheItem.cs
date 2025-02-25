using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kawapure.DuiCompiler.Parser.Preprocessor
{
    internal class IncludeGuardCacheItem : IIncludeCacheItem
    {
        // SourceFile is a PLACEHOLDER class!!!!!!!!
        // It should be replaced with the object for a preprocessor define
        // when the time comes for that to be implemented.
        protected WeakReference<SourceFile> _skipTarget;

        public IncludeGuardCacheItem(SourceFile a)
        {
            _skipTarget = new(a);
        }

        public bool ShouldSkip()
        {
            if (_skipTarget.TryGetTarget(out SourceFile? a))
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
