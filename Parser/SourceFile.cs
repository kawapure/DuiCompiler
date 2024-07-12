using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kawapure.DuiCompiler.Parser
{
    internal class SourceFile : ITextReaderSourceProvider
    {
        public enum FileType
        {
            DUI_UIFILE,
            PREPROCESSOR,
        }

        protected FileType m_fileType;
        protected TextReader m_mainTextReader;

        public string Path { get; protected set; }

        public string Contents { get; protected set; }

        public bool IsIncluded { get; protected set; } = false;

        public SourceFile? IncludingFile { get; private set; } = null;

        // TODO(izzy):
        // this should probably be refactored at some point
        // tokeniser should not be part of the source file
        // itself
        public Tokenizer m_tokenizer { get; protected set; }

        public SourceFile(string filePath, FileType fileType)
        {
            this.Path = filePath;
            this.Contents = File.ReadAllText(filePath);
            m_fileType = fileType;

            Tokenizer.AllowedLanguage tokenizerLanguages = m_fileType switch
            {
                // DirectUI input files (.ui, .uix, .xml)
                FileType.DUI_UIFILE =>
                    Tokenizer.AllowedLanguage.DUIXML | Tokenizer.AllowedLanguage.PREPROCESSOR,

                // Preprocessor input files (.h, .hpp, .hxx, .c, .cpp, .cxx)
                FileType.PREPROCESSOR =>
                    Tokenizer.AllowedLanguage.PREPROCESSOR,

                // Unknown file types should not be tokenized.
                _ => Tokenizer.AllowedLanguage.NONE
            };

            m_tokenizer = new Tokenizer(this, tokenizerLanguages);
            m_mainTextReader = new TextReader(this);
        }

        public FileType GetFileType()
        {
            return m_fileType;
        }

        /// <summary>
        /// Gets the main (shared) reader.
        /// </summary>
        public TextReader GetMainReader()
        {
            return m_mainTextReader;
        }

        /// <summary>
        /// Gets a new reader. If you're reading the file
        /// asynchronously (i.e. multithreaded environment), then you'll want
        /// to use this instead of the shared context.
        /// </summary>
        public TextReader GetNewReader(int defaultOffset = 0)
        {
            return new TextReader(this, defaultOffset);
        }
    }
}
