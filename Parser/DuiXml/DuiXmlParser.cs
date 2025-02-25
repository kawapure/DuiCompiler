using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kawapure.DuiCompiler.Parser.Preprocessor;

namespace Kawapure.DuiCompiler.Parser.DuiXml
{
    internal class DuiXmlParser
    {
        private readonly ITextReaderSourceProvider _sourceFile;
        private PreprocessorParser? _preprocessorParser = null;
        private WorldNode _world;

        public DuiXmlParser(ITextReaderSourceProvider sourceFile)
        {
            _sourceFile = sourceFile;

            // If we're coming from a SourceFile, then we want to evaluate the
            // preprocessor nodes in the file, so we execute the preprocessor
            // nodes first to modify the text.
            if (sourceFile is SourceFile)
            {
                _preprocessorParser = new PreprocessorParser((SourceFile)_sourceFile);
            }

            _world = new WorldNode(new SourceOrigin()
            {
                sourceProvider = _sourceFile,
                cursorOffset = 0,
            });
        }


    }
}
