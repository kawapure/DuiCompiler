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
            None         = 0b0000,
            DuiXml       = 0b0001,
            Preprocessor = 0b0010,
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
            ParsingSymbol,

            /// <summary>
            /// Interpret all textual data until a given terminator as an
            /// embedded string format; copy textual data mostly as-is.
            /// </summary>
            ParsingString,

            /// <summary>
            /// Ignore all textual data until a given terminator.
            /// </summary>
            ParsingComment,
        }

        /// <summary>
        /// 
        /// Result command from a reader function called in
        /// <see cref="Tokenize"/>.
        /// 
        /// </summary>
        protected enum ReaderCommand
        {
            Pass,
            Break,
        }

        //---------------------------------------------------------------------

        protected TokenizerMode GetTokenTargetMode(string token)
        {
            return token switch
            {
                "\"" => TokenizerMode.ParsingString,
                "\'" => TokenizerMode.ParsingString,
                "//" => TokenizerMode.ParsingComment,

                // "<" opens a type of quoted string literal for the
                // preprocessor, but should be parsed as symbolic when we're
                // targeting DUIXML.
                "<" => _fTargetingPreprocessor
                    ? TokenizerMode.ParsingString
                    : TokenizerMode.ParsingSymbol,

                // "*/" doesn't mean anything outside of a comment starting
                // with "/*"
                "/*" => TokenizerMode.ParsingComment,

                _ => TokenizerMode.ParsingSymbol,
            };
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The list of tokens to output.
        /// </summary>
        protected List<Token> _tokenList = new();

        //---------------------------------------------------------------------

        /// <summary>
        /// The source file we're reading from.
        /// </summary>
        protected ITextReaderSourceProvider _sourceFile;

        /// <summary>
        /// The main text reader object to use.
        /// </summary>
        protected TextReader _reader;

        //---------------------------------------------------------------------

        protected AllowedLanguage _allowedLanguages = AllowedLanguage.None;

        /// <summary>
        /// Allow the tokenizer to produce DUI XML tokens.
        /// </summary>
        /// <remarks>
        /// DUI XML shouldn't be tokenized when we're targeting C header files,
        /// so we will set this flag when that's the case.
        /// </remarks>
        protected bool _fAllowDuiXml
        {
            get
            {
                return (_allowedLanguages & AllowedLanguage.DuiXml) != AllowedLanguage.None;
            }

            set
            {
                if (value == true)
                {
                    _allowedLanguages |= AllowedLanguage.DuiXml;
                }
                else
                {
                    _allowedLanguages &= ~AllowedLanguage.DuiXml;
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
        protected bool _fAllowPreprocessor
        {
            get
            {
                return (_allowedLanguages & AllowedLanguage.Preprocessor) != AllowedLanguage.None;
            }

            set
            {
                if (value == true)
                {
                    _allowedLanguages |= AllowedLanguage.Preprocessor;
                }
                else
                {
                    _allowedLanguages &= ~AllowedLanguage.Preprocessor;
                }
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The current mode of the parser. See <see cref="TokenizerMode"/>.
        /// </summary>
        protected TokenizerMode _mode = TokenizerMode.ParsingSymbol;

        /// <summary>
        /// Was the mode just switched?
        /// </summary>
        /// <remarks>
        /// This is used by some mode functions in order to perform setup
        /// tasks.
        /// </remarks>
        protected bool _fModeJustSwitched = false;

        //---------------------------------------------------------------------

        /// <summary>
        /// A buffer used for processing tokens longer than one character.
        /// </summary>
        /// <remarks>
        /// This is used for parsing language keywords, user-defined symbols,
        /// numbers, string literals, and anything of the sort.
        /// </remarks>
        protected StringBuilder _stringBuffer = new();

        /// <summary>
        /// The file index at which <see cref="_stringBuffer"/> begins.
        /// </summary>
        protected uint _stringBufferOrigin = 0;

        //---------------------------------------------------------------------

        /// <summary>
        /// True if the target is the preprocessor language, false if the
        /// target is DUIXML.
        /// </summary>
        protected bool _fTargetingPreprocessor = false;

        /// <summary>
        /// Did we parse any whitespace characters on the current line?
        /// </summary>
        protected bool _fParsedAnyNonWhitespaceOnLine = false;

        /// <summary>
        /// Stores the last non-whitespace character read.
        /// </summary>
        /// <remarks>
        /// This is used for checking if the the preprocessor should continue
        /// the logical line past a line break, i.e. "\" at the end of a line.
        /// </remarks>
        protected char _lastNonWhitespaceChar = char.MinValue;

        //---------------------------------------------------------------------

        /// <summary>
        /// 
        /// Stores the opening quote character. This is used by
        /// <see cref="TokenizeString"/> to know when to terminate parsing.
        /// 
        /// </summary>
        protected char _openingQuoteChar = char.MinValue;

        /// <summary>
        /// Used by the string parser to remember if the next character should
        /// be escaped.
        /// </summary>
        protected bool _fParsingEscapeSequence = false;

        /// <summary>
        /// Stores the opening comment token to determine the relevant
        /// terminator token sequence.
        /// </summary>
        protected string _openingCommentToken = string.Empty;

        //---------------------------------------------------------------------

        public Tokenizer(ITextReaderSourceProvider sourceFile, AllowedLanguage allowedLanguages)
        {
            _sourceFile = sourceFile;
            _reader = sourceFile.GetNewReader();
            _allowedLanguages = allowedLanguages;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Flushes the string buffer if there is any content in it.
        /// </summary>
        protected void FlushStringBufferIfPossible()
        {
            if (_stringBuffer.Length > 0)
            {
                string? finalString = _stringBuffer.ToString();

                if (finalString != null)
                {
                    // Flush the string buffer to a new token.
                    Token tokenFromStringBuffer = new(
                        finalString,
                        _sourceFile,
                        _stringBufferOrigin,
                        GetTokenTypeFromMode(),
                        GetAppropriateTokenLanguage()
                    );
                    AddTokenToTokenList(tokenFromStringBuffer);

                    // Reset the string buffer for the next iteration:
                    _stringBufferOrigin = 0;
                    _stringBuffer.Clear();
                }
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Set the current mode of the tokenizer.
        /// </summary>
        protected void SetMode(TokenizerMode mode)
        {
            _mode = mode;
            _fModeJustSwitched = true;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Get the appropriate type for the destination token from the current
        /// tokenizer mode.
        /// </summary>
        protected Token.TokenType GetTokenTypeFromMode()
        {
            return _mode switch
            {
                TokenizerMode.ParsingSymbol =>
                    Token.TokenType.Symbol,

                TokenizerMode.ParsingString =>
                    Token.TokenType.StringLiteral,

                TokenizerMode.ParsingComment => throw new Exception(
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
        /// Adds a token to the <see cref="_tokenList">token list</see> if it
        /// is appropriate (the language is supported by the tokenizer session).
        /// </summary>
        /// <returns>
        /// True if the token was added to the list, false if it was rejected.
        /// </returns>
        protected bool AddTokenToTokenList(Token token)
        {
            if (
                (token._language == Token.TokenLanguage.DuiXml && _fAllowDuiXml) ||
                (token._language == Token.TokenLanguage.Preprocessor && _fAllowPreprocessor)
            )
            {
                _tokenList.Add(token);
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
            return _fTargetingPreprocessor
                ? Token.TokenLanguage.Preprocessor
                : Token.TokenLanguage.DuiXml;
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
                (TextReader.Status cs, char character) = _reader.Peek(0);
                
                if (
                    _fAllowPreprocessor &&
                    character == '\n' && _lastNonWhitespaceChar != '\\'
                )
                {
                    // New lines are significant for the preprocessor language
                    // and not for XML. We only consider them to terminate
                    // preprocessor tokenization and to test for when # is
                    // legal.

                    // When the last non-whitespace character is "\", then we
                    // continue the next line as though there were no line
                    // break at all, so we skip this operation.

                    _fTargetingPreprocessor = false;
                    _lastNonWhitespaceChar = char.MinValue;
                    _fParsedAnyNonWhitespaceOnLine = false;

                    Token newlineToken = new(
                        "\n",
                        _sourceFile,
                        (uint)_reader.GetCurrentOffset() - 1,
                        Token.TokenType.Symbol,
                        Token.TokenLanguage.Preprocessor
                    );
                    _tokenList.Add(newlineToken);

                    // FALL THROUGH
                    // We still need a \n token for parsing preprocessor
                    // directives.
                }

                ReaderCommand rc = _mode switch
                {
                    TokenizerMode.ParsingSymbol =>
                        TokenizeSymbolic(),

                    TokenizerMode.ParsingString =>
                        TokenizeString(),

                    TokenizerMode.ParsingComment =>
                        TokenizeComment(),

                    // This should never be possible.
                    _ => throw new NotImplementedException(
                        "Invalid tokenizer mode."
                    )
                };

                if (rc == ReaderCommand.Break)
                {
                    break;
                }
            }

            // Also flush the string buffer once the loop exits so that the
            // file may end on a non-special-character token:
            FlushStringBufferIfPossible();

            return _tokenList;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Tokenizes symbolic source-code content.
        /// </summary>
        protected ReaderCommand TokenizeSymbolic()
        {
            // We ignore this state, so we just always set it to false:
            _fModeJustSwitched = false;

            (TextReader.Status trStatus, char character) = _reader.Read();

            if (trStatus == TextReader.Status.OutOfBounds)
            {
                return ReaderCommand.Break;
            }

            bool isCharAscii = (character <= sbyte.MaxValue);
            bool isCharSimpleToken = (isCharAscii) && !char.IsLetterOrDigit(character) && character != '_';

            if (Char.IsWhiteSpace(character))
            {
                // White space isn't stored as a token object. It is simply
                // interpreted by the tokenizer as a terminator.
                FlushStringBufferIfPossible();
                return ReaderCommand.Pass;
            }
            else if (_fAllowPreprocessor && character != '#')
            {
                // We don't want to run the above code if the character is a
                // "#" character because that would break tokenization for
                // the preprocessor.
                _fParsedAnyNonWhitespaceOnLine = true;
                _lastNonWhitespaceChar = character;
            }
            
            if (_fAllowPreprocessor && character == '#' && !_fParsedAnyNonWhitespaceOnLine)
            {
                // "#" begins preprocessor parsing mode if no other characters
                // precede it.
                _fTargetingPreprocessor = true;

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
                (TextReader.Status lookaheadStatus, char nextChar) = _reader.Peek(0);

                if (lookaheadStatus == TextReader.Status.Success)
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
                (_fTargetingPreprocessor && character == '<')
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
                    _sourceFile,

                    // We substract the previously read amount from the cursor:
                    (uint)_reader.GetCurrentOffset() - 1,
                    Token.TokenType.Symbol,
                    GetAppropriateTokenLanguage()
                );
                AddTokenToTokenList(token);

                // Change the mode:
                TokenizerMode newMode = GetTokenTargetMode(modeSetToken);
                Debug.Assert(newMode != TokenizerMode.ParsingSymbol);
                SetMode(newMode);

                // Rewind so that the tokenizer mode begins at the token
                // (this is used for setting the quote character and for
                // validating the comment opening sequence):
                _reader.Rewind(rewindAmount);

                return ReaderCommand.Pass;
            }

            if (isCharSimpleToken)
            {
                // Flush the string buffer because we're now done
                // handling it:
                FlushStringBufferIfPossible();

                Token token = new(
                    character.ToString(),
                    _sourceFile,

                    // We substract the previously read amount from the cursor:
                    (uint)_reader.GetCurrentOffset() - 1,
                    Token.TokenType.Symbol,
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
                _stringBuffer.Append(character.ToString());

                if (_stringBufferOrigin == 0)
                    _stringBufferOrigin = (uint)_reader.GetCurrentOffset() - 1;
            }

            return ReaderCommand.Pass;
        }

        /// <summary>
        /// Tokenizes string literals. 
        /// </summary>
        protected ReaderCommand TokenizeString()
        {
            (TextReader.Status trStatus, char character) = _reader.Read();

            if (_fModeJustSwitched)
            {
                // If we just switched the mode, then we'll want to store the
                // last quote character to we can remember which terminator
                // to use for subsequent iterations.
                _openingQuoteChar = character;

                // We also want to set the string buffer origin so we can trace
                // the source of the string.
                _stringBufferOrigin = (uint)_reader.GetCurrentOffset() - 1;

                Debug.Assert(character == '\"' || character == '\'' || character == '<');
                Debug.Assert(_stringBuffer.Length == 0);

                // Now reset the flag because we're done with it for now and
                // we don't want it to bleed into future iterations at all.
                _fModeJustSwitched = false;

                // Because the below code will terminate, we would essentially
                // terminate immediately upon parsing the first string literal.
                // For this reason, we're going to exit now.
                return ReaderCommand.Pass;
            }

            if (trStatus == TextReader.Status.OutOfBounds)
            {
                // In this case, it's an unterminated string literal, and we
                // want to notify the user of this case.
                return ReaderCommand.Break;
            }

            if (_fParsingEscapeSequence)
            {
                switch (character)
                {
                    // \" escaped double quote sequence
                    case '"':
                        _stringBuffer.Append('\"');
                        break;
                    // \' escaped single quote sequence
                    case '\'':
                        _stringBuffer.Append('\'');
                        break;
                    // \t tab sequence
                    case 't':
                        _stringBuffer.Append('\t');
                        break;
                    // \n newline sequence
                    case 'n':
                        _stringBuffer.Append('\n');
                        break;
                    // \\ backslash sequence
                    case '\\':
                        _stringBuffer.Append('\\');
                        break;

                    default:
                        _stringBuffer.Append(character);
                        break;
                }

                _fParsingEscapeSequence = false;
                return ReaderCommand.Pass;
            }
            else if (character == '\\')
            {
                // In this case, we're going to begin an escape sequence.
                _fParsingEscapeSequence = true;
            }
            else if (character == _openingQuoteChar || _openingQuoteChar == '<' && character == '>')
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
                SetMode(TokenizerMode.ParsingSymbol);

                // We also want to put the terminator character as a token:
                Token terminatorToken = new(
                    character.ToString(),
                    _sourceFile,
                    // We substract the previously read amount from the cursor:
                    (uint)_reader.GetCurrentOffset() - 1,
                    Token.TokenType.Symbol,
                    GetAppropriateTokenLanguage()
                );
                AddTokenToTokenList(terminatorToken);

                return ReaderCommand.Pass;
            }

            // Finally, since none of the above checks have passed, we will
            // simply place the current character in the string buffer.
            _stringBuffer.Append(character);

            return ReaderCommand.Pass;
        }

        /// <summary>
        /// Skips over comments.
        /// </summary>
        protected ReaderCommand TokenizeComment()
        {
            // We ignore this state, so we just always set it to false:
            _fModeJustSwitched = false;

            int openingSequenceLen = _openingCommentToken.Length;

            for (int i = 0; i < openingSequenceLen; i++)
            {
                (TextReader.Status cs, char c) = _reader.Read();

                Debug.Assert(cs == TextReader.Status.Success);
                Debug.Assert(c == _openingCommentToken[i]);
            }

            // Get the comment terminator character and length:
            string terminator = _openingCommentToken switch
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
                (TextReader.Status cs, char c) = _reader.Read();

                if (cs == TextReader.Status.Success)
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

            SetMode(TokenizerMode.ParsingSymbol);

            return ReaderCommand.Pass;
        }
    }
}
