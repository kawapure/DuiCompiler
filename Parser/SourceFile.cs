using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kawapure.DuiCompiler.Parser
{
    /// <summary>
    /// Provides information about source code text files.
    /// </summary>
    internal class SourceFile : ITextReaderSourceProvider
    {
        /// <summary>
        /// The file type of a source file.
        /// </summary>
        public enum FileType
        {
            /// <summary>
            /// The file is a DirectUI DUIXML file.
            /// </summary>
            DUI_UIFILE,

            /// <summary>
            /// The file is a C or C++ header file.
            /// </summary>
            PREPROCESSOR,
        }

        /// <summary>
        /// The type of this source file.
        /// </summary>
        protected FileType m_fileType;

        /// <summary>
        /// Manages retrieval of line/column offsets from this file.
        /// </summary>
        protected LineOffsetManager m_lineOffsetManager;

        /// <summary>
        /// Stores the (currently relative) path of the file.
        /// </summary>
        /*
         * TODO: Change this to always store the absolute path of the file.
         */
        public string Path { get; protected set; }

        //---------------------------------------------------------------------------------------------------
        // ITextReaderSourceProvider:

        public string Contents { get; protected set; }

        public int[] GetLineOffsets() => m_lineOffsetManager.GetLineOffsets();

        //---------------------------------------------------------------------------------------------------

        /// <summary>
        /// Is the file included by another file via the preprocessor, or is it
        /// a top-level file.
        /// </summary>
        public bool IsIncluded { get; protected set; } = false;

        /// <summary>
        /// If the file is included, stores a reference to the including file.
        /// </summary>
        public SourceFile? IncludingFile { get; private set; } = null;

        // TODO(izzy):
        // this should probably be refactored at some point
        // tokeniser should not be part of the source file
        // itself
        public Tokenizer m_tokenizer { get; protected set; }

        //---------------------------------------------------------------------------------------------------

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
            m_lineOffsetManager = new(this.Contents);
        }

        //---------------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the type of this file.
        /// </summary>
        public FileType GetFileType()
        {
            return m_fileType;
        }

        /// <summary>
        /// Gets a new reader for this file.
        /// </summary>
        public TextReader GetNewReader(int defaultOffset = 0)
        {
            return new TextReader(this, defaultOffset);
        }
    }
}
