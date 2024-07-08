using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Kawapure.DuiCompiler.Parser
{
    /// <summary>
    /// 
    /// Breaks source code files up into easy-to-digest tokens to make parsing
    /// easier.
    /// 
    /// </summary>
    /// 
    /// <remarks>
    /// 
    /// <para>
    /// 
    /// Tokens are implemented in a fairly language-agnostic manner here. The
    /// only opinionated part of the abstract token parser is that it breaks up
    /// strings of alphanumeric characters (and any characters unavailable in
    /// ASCII) into single tokens.
    /// 
    /// </para>
    /// 
    /// <para>
    /// 
    /// To put it another way: this parser only allows parsing languages which
    /// only have special characters in the ASCII range; non-ASCII characters
    /// are always clustered together, and thus cannot be used as language
    /// tokens reliably.
    /// 
    /// </para>
    /// 
    /// <para>
    /// 
    /// Since this tokenizer is designed to target the C preprocessor and
    /// XML-like markup, non-symbolic tokens are somewhat expected to be only
    /// one character long per-token. Thus, no maximal munch principle is used.
    /// 
    /// </para>
    /// 
    /// </remarks>
    internal class Tokenizer
    {
        [Flags]
        public enum AllowedLanguage
        {
            NONE         = 0b0000,
            DUIXML       = 0b0001,
            PREPROCESSOR = 0b0010,
        }

        /// <summary>
        /// Mode for parsing the tokens.
        /// </summary>
        /// <remarks>
        /// The tokenizer mode controls how tokens are generated. For example,
        /// when parsing a string literal, we want to include all whitespace
        /// because it is relevant to the meaning of that string.
        /// </remarks>
        protected enum TokenizerMode
        {
            /// <summary>
            /// Break up textual data into symbolic tokens for parsing source
            /// code.
            /// </summary>
            PARSING_SYMBOL,

            /// <summary>
            /// Interpret all textual data until a given terminator as an
            /// embedded string format; copy textual data mostly as-is.
            /// </summary>
            PARSING_STRING,

            /// <summary>
            /// Ignore all textual data until a given terminator.
            /// </summary>
            PARSING_COMMENT,
        }

        /// <summary>
        /// 
        /// Result command from a reader function called in
        /// <see cref="Tokenize"/>.
        /// 
        /// </summary>
        protected enum ReaderCommand
        {
            PASS,
            BREAK,
        }

        //---------------------------------------------------------------------

        protected TokenizerMode GetTokenTargetMode(string token)
        {
            return token switch
            {
                "\"" => TokenizerMode.PARSING_STRING,
                "\'" => TokenizerMode.PARSING_STRING,
                "//" => TokenizerMode.PARSING_COMMENT,

                // "<" opens a type of quoted string literal for the
                // preprocessor, but should be parsed as symbolic when we're
                // targeting DUIXML.
                "<" => m_bTargetingPreprocessor
                    ? TokenizerMode.PARSING_STRING
                    : TokenizerMode.PARSING_SYMBOL,

                // "*/" doesn't mean anything outside of a comment starting
                // with "/*"
                "/*" => TokenizerMode.PARSING_COMMENT,

                _ => TokenizerMode.PARSING_SYMBOL,
            };
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The list of tokens to output.
        /// </summary>
        protected List<Token> m_tokenList = new();

        //---------------------------------------------------------------------

        /// <summary>
        /// The source file we're reading from.
        /// </summary>
        protected ITextReaderSourceProvider m_sourceFile;

        /// <summary>
        /// The main text reader object to use.
        /// </summary>
        protected TextReader m_reader;

        //---------------------------------------------------------------------

        protected AllowedLanguage m_allowedLanguages = AllowedLanguage.NONE;

        /// <summary>
        /// Allow the tokenizer to produce DUI XML tokens.
        /// </summary>
        /// <remarks>
        /// DUI XML shouldn't be tokenized when we're targeting C header files,
        /// so we will set this flag when that's the case.
        /// </remarks>
        protected bool m_bAllowDuiXml
        {
            get
            {
                return (m_allowedLanguages & AllowedLanguage.DUIXML) != AllowedLanguage.NONE;
            }

            set
            {
                if (value == true)
                {
                    m_allowedLanguages |= AllowedLanguage.DUIXML;
                }
                else
                {
                    m_allowedLanguages &= ~AllowedLanguage.DUIXML;
                }
            }
        }

        /// <summary>
        /// Allow the tokenizer to produce preprocessor tokens.
        /// </summary>
        /// <remarks>
        /// We don't parse preprocessor tokens when parsing quoted attribute
        /// strings in DUIXML files. Preprocessor tokens should never occur
        /// in such a situation.
        /// </remarks>
        protected bool m_bAllowPreprocessor
        {
            get
            {
                return (m_allowedLanguages & AllowedLanguage.PREPROCESSOR) != AllowedLanguage.NONE;
            }

            set
            {
                if (value == true)
                {
                    m_allowedLanguages |= AllowedLanguage.PREPROCESSOR;
                }
                else
                {
                    m_allowedLanguages &= ~AllowedLanguage.PREPROCESSOR;
                }
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The current mode of the parser. See <see cref="TokenizerMode"/>.
        /// </summary>
        protected TokenizerMode m_mode = TokenizerMode.PARSING_SYMBOL;

        /// <summary>
        /// Was the mode just switched?
        /// </summary>
        /// <remarks>
        /// This is used by some mode functions in order to perform setup
        /// tasks.
        /// </remarks>
        protected bool m_bModeJustSwitched = false;

        //---------------------------------------------------------------------

        /// <summary>
        /// A buffer used for processing tokens longer than one character.
        /// </summary>
        protected StringBuilder m_stringBuffer = new();

        /// <summary>
        /// The file index at which <see cref="m_stringBuffer"/> begins.
        /// </summary>
        protected uint m_stringBufferOrigin = 0;

        //---------------------------------------------------------------------

        /// <summary>
        /// True if the target is the preprocessor language, false if the
        /// target is DUIXML.
        /// </summary>
        protected bool m_bTargetingPreprocessor = false;

        /// <summary>
        /// Did we parse any whitespace characters on the current line?
        /// </summary>
        protected bool m_bParsedAnyNonWhitespaceOnLine = false;

        /// <summary>
        /// Stores the last non-whitespace character read.
        /// </summary>
        /// <remarks>
        /// This is used for checking if the the preprocessor should continue
        /// the logical line past a line break, i.e. "\" at the end of a line.
        /// </remarks>
        protected char m_lastNonWhitespaceChar = char.MinValue;

        //---------------------------------------------------------------------

        protected char m_openingQuoteChar = char.MinValue;
        protected bool m_bParsingEscapeSequence = false;
        protected string m_openingCommentToken = string.Empty;

        //---------------------------------------------------------------------

        public Tokenizer(ITextReaderSourceProvider sourceFile, AllowedLanguage allowedLanguages)
        {
            m_sourceFile = sourceFile;
            m_reader = sourceFile.GetNewReader();
            m_allowedLanguages = allowedLanguages;
        }

        //---------------------------------------------------------------------

        protected void FlushStringBufferIfPossible()
        {
            if (m_stringBuffer.Length > 0)
            {
                string? finalString = m_stringBuffer.ToString();

                if (finalString != null)
                {
                    // Flush the string buffer to a new token.
                    Token tokenFromStringBuffer = new(
                        finalString,
                        m_sourceFile,
                        m_stringBufferOrigin,
                        GetTokenTypeFromMode(),
                        GetAppropriateTokenLanguage()
                    );
                    AddTokenToTokenList(tokenFromStringBuffer);

                    // Reset the string buffer for the next iteration:
                    m_stringBufferOrigin = 0;
                    m_stringBuffer.Clear();
                }
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Set the current mode of the tokenizer.
        /// </summary>
        protected void SetMode(TokenizerMode mode)
        {
            m_mode = mode;
            m_bModeJustSwitched = true;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Get the appropriate type for the destination token from the current
        /// tokenizer mode.
        /// </summary>
        protected Token.TokenType GetTokenTypeFromMode()
        {
            return m_mode switch
            {
                TokenizerMode.PARSING_SYMBOL =>
                    Token.TokenType.SYMBOL,

                TokenizerMode.PARSING_STRING =>
                    Token.TokenType.STRING_LITERAL,

                TokenizerMode.PARSING_COMMENT => throw new Exception(
                    "Attempted to call GetTokenTypeFromMode() in the comment " +
                    "tokenizer mode. Did you try flushing after the mode " +
                    "changed, or did you forget to change the mode itself?"
                ),

                _ => throw new Exception(
                    "No mapping for the the desired token type exists for the " +
                    "current tokenizer mode."
                ),
            };
        }

        /// <summary>
        /// Adds a token to the <see cref="m_tokenList">token list</see> if it
        /// is appropriate (the language is supported by the tokenizer session).
        /// </summary>
        /// <returns>
        /// True if the token was added to the list, false if it was rejected.
        /// </returns>
        protected bool AddTokenToTokenList(Token token)
        {
            if (
                (token.m_language == Token.TokenLanguage.DUIXML && m_bAllowDuiXml) ||
                (token.m_language == Token.TokenLanguage.PREPROCESSOR && m_bAllowPreprocessor)
            )
            {
                m_tokenList.Add(token);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the appropriate token language based on the current parsing
        /// language of the tokenizer.
        /// </summary>
        protected Token.TokenLanguage GetAppropriateTokenLanguage()
        {
            return m_bTargetingPreprocessor
                ? Token.TokenLanguage.PREPROCESSOR
                : Token.TokenLanguage.DUIXML;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Tokenize the input and send the output.
        /// </summary>
        /// <returns>A list of tokens.</returns>
        public List<Token> Tokenize()
        {
            while (true)
            {
                // Read the current character:
                (TextReader.Status cs, char character) = m_reader.Peek(0);
                
                if (character == '\n' && m_lastNonWhitespaceChar != '\\')
                {
                    // New lines are significant for the preprocessor language
                    // and not for XML. We only consider them to terminate
                    // preprocessor tokenization and to test for when # is
                    // legal.

                    // When the last non-whitespace character is "\", then we
                    // continue the next line as though there were no line
                    // break at all, so we skip this operation.

                    m_bTargetingPreprocessor = false;
                    m_lastNonWhitespaceChar = char.MinValue;
                    m_bParsedAnyNonWhitespaceOnLine = false;

                    // FALL THROUGH
                    // We still need a \n token for parsing preprocessor
                    // directives.
                }

                ReaderCommand rc = m_mode switch
                {
                    TokenizerMode.PARSING_SYMBOL =>
                        TokenizeSymbolic(),

                    TokenizerMode.PARSING_STRING =>
                        TokenizeString(),

                    TokenizerMode.PARSING_COMMENT =>
                        TokenizeComment(),

                    // This should never be possible.
                    _ => throw new NotImplementedException(
                        "Invalid tokenizer mode."
                    )
                };

                if (rc == ReaderCommand.BREAK)
                {
                    break;
                }
            }

            // Also flush the string buffer once the loop exits so that the
            // file may end on a non-special-character token:
            FlushStringBufferIfPossible();

            return m_tokenList;
        }

        //---------------------------------------------------------------------

        protected ReaderCommand TokenizeSymbolic()
        {
            // We ignore this state, so we just always set it to false:
            m_bModeJustSwitched = false;

            (TextReader.Status trStatus, char character) = m_reader.Read();

            if (trStatus == TextReader.Status.OUT_OF_BOUNDS)
            {
                return ReaderCommand.BREAK;
            }

            bool isCharAscii = (character <= sbyte.MaxValue);
            bool isCharSimpleToken = (isCharAscii) && !char.IsLetterOrDigit(character);

            if (Char.IsWhiteSpace(character))
            {
                // White space isn't stored as a token object. It is simply
                // interpreted by the tokenizer as a terminator.
                FlushStringBufferIfPossible();
                return ReaderCommand.PASS;
            }
            else if (m_bAllowPreprocessor && character != '#')
            {
                // We don't want to run the above code if the character is a
                // "#" character because that would break tokenization for
                // the preprocessor.
                m_bParsedAnyNonWhitespaceOnLine = true;
                m_lastNonWhitespaceChar = character;
            }
            
            if (character == '#' && !m_bParsedAnyNonWhitespaceOnLine)
            {
                // "#" begins preprocessor parsing mode if no other characters
                // precede it.
                m_bTargetingPreprocessor = true;

                // We FALL THROUGH here. After all, this state only changes
                // how the tokenizer behaves, not how the parser interprets
                // the resulting tokens. As such, we want to include the
                // character as a token.
            }

            // If the character is a forward slash, then we need to
            // look ahead to the next character to determine if we're
            // parsing a comment start token:
            char forwardSlashNextCharacter = char.MinValue;
            if (character == '/')
            {
                // By default, we don't really want to go through 
                (TextReader.Status lookaheadStatus, char nextChar) = m_reader.Peek(0);

                if (lookaheadStatus == TextReader.Status.SUCCESS)
                {
                    forwardSlashNextCharacter = nextChar;
                }
            }

            // First, we'll check to handle some specific cases which can
            // switch the tokenizer mode:
            if (
                character == '\'' ||
                character == '"' ||
                character == '\'' ||
                character == '/' && (
                        forwardSlashNextCharacter == '/' ||
                        forwardSlashNextCharacter == '*'
                    ) ||
                (m_bTargetingPreprocessor && character == '<')
            )
            {
                // For quote characters, these are only one character long,
                // so we don't need to do any looking ahead. We just
                // convert the type to a string and look up the new mode
                // with that:
                string modeSetToken = character.ToString();
                int rewindAmount = 1;

                if (character == '/')
                {
                    // Because we're now interpreting multi-character
                    // tokens, we need to set the rewind amount to
                    // the length of these tokens (I just decided to
                    // hardcode 2 lol)
                    rewindAmount = 2;

                    // The token to be used for querying the mode to which to
                    // change is also multiple characters now, so we need to
                    // concatenate the two we stored here:
                    modeSetToken =
                        character.ToString() +
                        forwardSlashNextCharacter.ToString();
                }

                // Because we're switching the mode, we need to finish up
                // whatever work we're doing here:
                FlushStringBufferIfPossible();

                // We still want to put the current token down, because the
                // parser is going to need it to make sense of the succeeding
                // token.
                Token token = new(
                    character.ToString(),
                    m_sourceFile,

                    // We substract the previously read amount from the cursor:
                    (uint)m_reader.GetCurrentOffset() - 1,
                    Token.TokenType.SYMBOL,
                    GetAppropriateTokenLanguage()
                );
                AddTokenToTokenList(token);

                // Change the mode:
                TokenizerMode newMode = GetTokenTargetMode(modeSetToken);
                Debug.Assert(newMode != TokenizerMode.PARSING_SYMBOL);
                SetMode(newMode);

                // Rewind so that the tokenizer mode begins at the token
                // (this is used for setting the quote character and for
                // validating the comment opening sequence):
                m_reader.Rewind(rewindAmount);

                return ReaderCommand.PASS;
            }

            if (isCharSimpleToken)
            {
                // Flush the string buffer because we're now done
                // handling it:
                FlushStringBufferIfPossible();

                Token token = new(
                    character.ToString(),
                    m_sourceFile,

                    // We substract the previously read amount from the cursor:
                    (uint)m_reader.GetCurrentOffset() - 1,
                    Token.TokenType.SYMBOL,
                    GetAppropriateTokenLanguage()
                );
                AddTokenToTokenList(token);
            }
            else
            {
                // In this case, we don't know the length of the following
                // string, but we want to consider the whole string as a
                // single token, so we add it to the string buffer and
                // flush it later:
                m_stringBuffer.Append(character.ToString());
                m_stringBufferOrigin = (uint)m_reader.GetCurrentOffset() - 1;
            }

            return ReaderCommand.PASS;
        }

        protected ReaderCommand TokenizeString()
        {
            (TextReader.Status trStatus, char character) = m_reader.Read();

            if (m_bModeJustSwitched)
            {
                // If we just switched the mode, then we'll want to store the
                // last quote character to we can remember which terminator
                // to use for subsequent iterations.
                m_openingQuoteChar = character;

                Debug.Assert(character == '\"' || character == '\'' || character == '<');
                Debug.Assert(m_stringBuffer.Length == 0);

                // Now reset the flag because we're done with it for now and
                // we don't want it to bleed into future iterations at all.
                m_bModeJustSwitched = false;

                // Because the below code will terminate, we would essentially
                // terminate immediately upon parsing the first string literal.
                // For this reason, we're going to exit now.
                return ReaderCommand.PASS;
            }

            if (trStatus == TextReader.Status.OUT_OF_BOUNDS)
            {
                // In this case, it's an unterminated string literal, and we
                // want to notify the user of this case.
                return ReaderCommand.BREAK;
            }

            if (m_bParsingEscapeSequence)
            {
                switch (character)
                {
                    // \" escaped double quote sequence
                    case '"':
                        m_stringBuffer.Append('\"');
                        break;
                    // \' escaped single quote sequence
                    case '\'':
                        m_stringBuffer.Append('\'');
                        break;
                    // \t tab sequence
                    case 't':
                        m_stringBuffer.Append('\t');
                        break;
                    // \n newline sequence
                    case 'n':
                        m_stringBuffer.Append('\n');
                        break;
                    // \\ backslash sequence
                    case '\\':
                        m_stringBuffer.Append('\\');
                        break;

                    default:
                        m_stringBuffer.Append(character);
                        break;
                }

                m_bParsingEscapeSequence = false;
                return ReaderCommand.PASS;
            }
            else if (character == '\\')
            {
                // In this case, we're going to begin an escape sequence.
                m_bParsingEscapeSequence = true;
            }
            else if (character == m_openingQuoteChar || m_openingQuoteChar == '<' && character == '>')
            {
                /*
                 * The opening quote character is (almost) always the terminator,
                 * so if we end up here (note that the escape sequence code
                 * above skips this code), then we will terminate the quote
                 * and reset to token parsing mode.
                 * 
                 * NOTE: I did a hack for C preprocessor use-include-directory
                 * quotes (<>), which is the only time the terminator character
                 * will differ from the opening character.
                 */
                FlushStringBufferIfPossible();
                SetMode(TokenizerMode.PARSING_SYMBOL);

                // We also want to put the terminator character as a token:
                Token terminatorToken = new(
                    character.ToString(),
                    m_sourceFile,
                    // We substract the previously read amount from the cursor:
                    (uint)m_reader.GetCurrentOffset() - 1,
                    Token.TokenType.SYMBOL,
                    GetAppropriateTokenLanguage()
                );
                AddTokenToTokenList(terminatorToken);

                return ReaderCommand.PASS;
            }

            // Finally, since none of the above checks have passed, we will
            // simply place the current character in the string buffer.
            m_stringBuffer.Append(character);

            return ReaderCommand.PASS;
        }

        protected ReaderCommand TokenizeComment()
        {
            // We ignore this state, so we just always set it to false:
            m_bModeJustSwitched = false;

            int openingSequenceLen = m_openingCommentToken.Length;

            for (int i = 0; i < openingSequenceLen; i++)
            {
                (TextReader.Status cs, char c) = m_reader.Read();

                Debug.Assert(cs == TextReader.Status.SUCCESS);
                Debug.Assert(c == m_openingCommentToken[i]);
            }

            // Get the comment terminator character and length:
            string terminator = m_openingCommentToken switch
            {
                "/*" => "*/",
                "//" => "\n",
                _ => throw new NotImplementedException(
                    "Invalid comment opening token."
                )
            };
            int terminatorLen = terminator.Length;
            char[] terminatorBuffer = new char[terminatorLen];

            while (true)
            {
                (TextReader.Status cs, char c) = m_reader.Read();

                if (cs == TextReader.Status.SUCCESS)
                {
                    // Shift the terminator buffer:
                    for (int i = terminatorLen; i > 0; i--)
                    {
                        terminatorBuffer[i - 1] = terminatorBuffer[i];
                    }
                    terminatorBuffer[terminatorLen - 1] = c;

                    // If the terminator buffer contains the terminator sequence,
                    // then break:
                    if (terminatorBuffer.ToString() == terminator)
                    {
                        break;
                    }
                }
            }

            SetMode(TokenizerMode.PARSING_SYMBOL);

            return ReaderCommand.PASS;
        }
    }
}
