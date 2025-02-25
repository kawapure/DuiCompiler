using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kawapure.DuiCompiler.Parser.Preprocessor.Node;

namespace Kawapure.DuiCompiler.Parser.Preprocessor
{
    /// <summary>
    /// Parses preprocessor token streams.
    /// </summary>
    internal class PreprocessorParser
    {
        public enum KeywordState
        {
            Invalid,
            Supported,
            Unsupported,
            PreprocessorOnly,
        }

        protected enum EInternalParseStatus
        {
            Success,
            Fail,
        }

        protected record struct InternalParseResult(EInternalParseStatus status, ParseNode? node);

        protected record struct SimpleParseData(Token keyword, Token firstArgument);

        protected readonly IncludeCache _pragmaOnceStore;
        protected readonly ITextReaderSourceProvider _sourceFile;
        protected WorldNode _world;
        protected ParseNode _currentParent;

        public PreprocessorParser(ITextReaderSourceProvider sourceFile)
        {
            _sourceFile = sourceFile;

            _world = new(new SourceOrigin()
            {
                sourceProvider = sourceFile,
                cursorOffset = 0,
            });
            _currentParent = _world;
        }

        public ParseNode? ParseTokenSequence(List<Token> tokens)
            => ParseTokenSequence(new TokenStream(tokens));

        public ParseNode? ParseTokenSequence(TokenStream tokens)
        {
            Debug.Assert(tokens[0]._language == Token.TokenLanguage.Preprocessor);

            // All valid preprocessor token sequences must begin with the "#"
            // opening character, so we enforce this here:
            if (tokens[0].ToString() != "#")
            {
                throw new ParseError(
                    $"Invalid preprocessor beginning token \"{tokens[0].ToSafeString()}\". " +
                    $"Please review the input in a debug build as this is almost certainly a" +
                    $"compiler error.",
                    tokens[0]._sourceOrigin
                );
            }

            KeywordState state = QueryKeywordState(tokens[1]);

            if (state == KeywordState.Supported || !IsParsingInDuiXmlFile() && state == KeywordState.PreprocessorOnly)
            {
                // If we get here, then we want to start the token stream at a
                // later position. We do not want to parse the # character again.
                tokens.position++;

                InternalParseResult result = DispatchKeywordParser(tokens[0], tokens);

                if (result.status == EInternalParseStatus.Success && result.node != null)
                {
                    return result.node;
                }
            }
            else if (IsParsingInDuiXmlFile() && state == KeywordState.PreprocessorOnly)
            {
                throw new ParseError(
                    $"The preprocessor command \"{tokens[1].ToSafeString()}\" is not" +
                    $"supported in DUIXML files. It is only supported in C header files, " +
                    $"where it is simply ignored.",
                    tokens[1]._sourceOrigin
                );
            }
            else if (state == KeywordState.Unsupported)
            {
                if (IsParsingInDuiXmlFile())
                {
                    throw new ParseError(
                        $"The preprocessor command \"{tokens[1].ToSafeString()}\" is " +
                        $"not supported by DuiCompiler.",
                        tokens[1]._sourceOrigin
                    );
                }
            }
            else if (state == KeywordState.Invalid)
            {
                if (IsParsingInDuiXmlFile())
                {
                    throw new ParseError(
                        $"Unknown preprocessor command \"{tokens[1].ToSafeString()}\".",
                        tokens[1]._sourceOrigin
                    );
                }
            }

            return null;
        }

        public KeywordState QueryKeywordState(Token token)
        {
            return QueryKeywordState(token.ToString());
        }

        public KeywordState QueryKeywordState(string keyword)
        {
            return keyword switch
            {
                "if"      => KeywordState.Supported,
                "ifdef"   => KeywordState.Supported,
                "ifndef"  => KeywordState.Supported,
                "elif"    => KeywordState.Supported,
                "else"    => KeywordState.Supported,
                "endif"   => KeywordState.Supported,
                "define"  => KeywordState.Supported,
                "undef"   => KeywordState.Supported,
                "include" => KeywordState.Supported,
                "pragma"  => KeywordState.Supported,
                "error"   => KeywordState.Supported,
                 
                "import"  => KeywordState.Unsupported,
                "line"    => KeywordState.Unsupported,
                "using"   => KeywordState.Unsupported,

                _ => KeywordState.Invalid
            };
        }

        protected bool IsParsingInDuiXmlFile()
        {
            if (_sourceFile is SourceFile f && f.GetFileType() == SourceFile.FileType.DuiUiFile)
            {
                return true;
            }

            return false;
        }

        protected InternalParseResult DispatchKeywordParser(Token initialKw, TokenStream tokenStream)
        {
            return initialKw.ToString() switch
            {
                "if"      => ParseIf(tokenStream),
                "ifdef"   => ParseIfDef(tokenStream),
                "ifndef"  => ParseIfNDef(tokenStream),
                "elif"    => ParseElIf(tokenStream),
                "else"    => ParseElse(tokenStream),
                "endif"   => ParseEndIf(tokenStream),
                "define"  => ParseDefine(tokenStream),
                "undef"   => ParseUndef(tokenStream),
                "include" => ParseInclude(tokenStream),
                "pragma"  => ParsePragma(tokenStream),
                "error"   => ParseErrorCommand(tokenStream),

                // Should never happen:
                _ => throw new NotImplementedException("DispatchKeywordParser invalid initial keyword."),
            };
        }

        /// <summary>
        /// Parse a "void statement": one takes zero arguments, or may be
        /// followed by exactly zero keywords.
        /// 
        /// This includes #else only.
        /// </summary>
        protected Token ParseVoidStatement(TokenStream tokenStream, string verificationKeyword)
        {
            Token initialToken = tokenStream.Next();

            Debug.Assert(initialToken.ToString().ToLower() != verificationKeyword);

            tokenStream.Expect(
                "\n",
                $"An {verificationKeyword} statement must be followed by a newline."
            );

            return initialToken;
        }

        /// <summary>
        /// Parse a "simple statement".
        /// 
        /// This includes #ifdef, #ifndef, and #undef, all of which take
        /// exactly one symbolic argument.
        /// </summary>
        protected SimpleParseData ParseSimpleStatement(TokenStream tokenStream, string initialKeyword)
        {
            Token initialToken = tokenStream.Next();

            Debug.Assert(initialKeyword.ToString().ToLower() == initialKeyword);

            Token firstArgumentToken = tokenStream.Next();

            // TODO: Validate that the first argument token is alphanumeric.

            tokenStream.Expect(
                "\n", 
                $"An {initialKeyword} statement must be followed by exactly only one symbol name."
            );

            return new SimpleParseData()
            {
                keyword = initialToken,
                firstArgument = firstArgumentToken
            };
        }

        protected InternalParseResult ParseIf(TokenStream tokenStream)
        {
            Debug.Assert(tokenStream[0].ToString().ToLower() == "if");

            return new InternalParseResult(EInternalParseStatus.Success, null);
        }

        protected InternalParseResult ParseIfDef(TokenStream tokenStream)
        {
            SimpleParseData data = ParseSimpleStatement(tokenStream, "ifdef");

            IfNode ifNode = new(data.keyword._sourceOrigin);

            ParseNode definedCheck = new("DefinedCheck", data.firstArgument._sourceOrigin);
            definedCheck.SetAttribute("Name", data.firstArgument.ToSafeString());

            ifNode.Expression.AppendChild(definedCheck);

            return new InternalParseResult(
                EInternalParseStatus.Success,
                ifNode
            );
        }

        protected InternalParseResult ParseIfNDef(TokenStream tokenStream)
        {
            SimpleParseData data = ParseSimpleStatement(tokenStream, "ifndef");

            IfNode ifNotNode = new(data.keyword._sourceOrigin);

            ParseNode notNode = new("UnaryNot", data.keyword._sourceOrigin);

            ParseNode definedCheck = new("DefinedCheck", data.firstArgument._sourceOrigin);
            definedCheck.SetAttribute("Name", data.firstArgument.ToSafeString());

            notNode.AppendChild(definedCheck);

            ifNotNode.Expression.AppendChild(notNode);

            return new InternalParseResult(
                EInternalParseStatus.Success,
                ifNotNode
            );
        }

        protected InternalParseResult ParseElIf(TokenStream tokenStream)
        {
            return new InternalParseResult(EInternalParseStatus.Success, null);
        }
        protected InternalParseResult ParseElse(TokenStream tokenStream)
        {
            Token token = ParseVoidStatement(tokenStream, "else");

            if (_currentParent.Name == "If")
            {
                if (_currentParent.ParentNode != null)
                {
                    _currentParent = _currentParent.ParentNode;
                }
                else
                {
                    throw new Exception("An if statement should not be orphaned.");
                }
            }
            else
            {
                // If we're not in an if statement at the moment, then it's
                // almost certain that the user put this token where it doesn't
                // belong.
                throw new ParseError(
                    "Unexpected #else outside of #if statement context.",
                    token._sourceOrigin
                );
            }

            return new InternalParseResult(EInternalParseStatus.Success, null);
        }

        protected InternalParseResult ParseEndIf(TokenStream tokenStream)
        {
            return new InternalParseResult(EInternalParseStatus.Success, null);
        }
        protected InternalParseResult ParseDefine(TokenStream tokenStream)
        {
            return new InternalParseResult(EInternalParseStatus.Success, null);
        }

        protected InternalParseResult ParseUndef(TokenStream tokenStream)
        {
            SimpleParseData data = ParseSimpleStatement(tokenStream, "undef");

            UndefNode undefNode = new(data.keyword._sourceOrigin, data.firstArgument.ToSafeString());

            return new InternalParseResult(
                EInternalParseStatus.Success,
                undefNode
            );
        }

        protected InternalParseResult ParseInclude(TokenStream tokenStream)
        {
            return new InternalParseResult(EInternalParseStatus.Success, null);
        }

        protected InternalParseResult ParsePragma(TokenStream tokenStream)
        {
            return new InternalParseResult(EInternalParseStatus.Success, null);
        }

        protected InternalParseResult ParseErrorCommand(TokenStream tokenStream)
        {
            return new InternalParseResult(EInternalParseStatus.Success, null);
        }

        /// <summary>
        /// Parse a preprocessor expression (i.e. in an #if condition)
        /// </summary>
        protected InternalParseResult ParseExpression(TokenStream tokenStream)
        {


            return new InternalParseResult(EInternalParseStatus.Success, null);
        }
    }
}
