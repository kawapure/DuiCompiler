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
        private readonly ITextReaderSourceProvider m_sourceFile;
        private PreprocessorParser? m_preprocessorParser = null;
        private WorldNode m_world;

        public DuiXmlParser(ITextReaderSourceProvider sourceFile)
        {
            m_sourceFile = sourceFile;

            // If we're coming from a SourceFile, then we want to evaluate the
            // preprocessor nodes in the file, so we execute the preprocessor
            // nodes first to modify the text.
            if (sourceFile is SourceFile)
            {
                m_preprocessorParser = new PreprocessorParser((SourceFile)m_sourceFile);
            }

            m_world = new WorldNode(new SourceOrigin()
            {
                sourceProvider = m_sourceFile,
                cursorOffset = 0,
            });
        }


    }
}
