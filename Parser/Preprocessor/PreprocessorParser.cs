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
            SUPPORTED,
            UNSUPPORTED,
            PREPROCESSOR_ONLY,
            INVALID,
        }

        protected enum EInternalParseStatus
        {
            SUCCESS,
            FAIL,
        }

        protected record struct InternalParseResult(EInternalParseStatus status, ParseNode? node);

        protected readonly IncludeCache m_pragmaOnceStore;
        protected readonly ITextReaderSourceProvider m_sourceFile;
        protected WorldNode m_world;
        protected ParseNode m_currentParent;

        public PreprocessorParser(ITextReaderSourceProvider sourceFile)
        {
            m_sourceFile = sourceFile;

            m_world = new(new SourceOrigin()
            {
                sourceProvider = sourceFile,
                cursorOffset = 0,
            });
            m_currentParent = m_world;
        }

        public ParseNode? ParseTokenSequence(List<Token> tokens)
            => ParseTokenSequence(new TokenStream(tokens));

        public ParseNode? ParseTokenSequence(TokenStream tokens)
        {
            Debug.Assert(tokens[0].m_language == Token.TokenLanguage.PREPROCESSOR);

            // All valid preprocessor token sequences must begin with the "#"
            // opening character, so we enforce this here:
            if (tokens[0].ToString() != "#")
            {
                throw new ParseError(
                    $"Invalid preprocessor beginning token \"{tokens[0].ToSafeString()}\". " +
                    $"Please review the input in a debug build as this is almost certainly a" +
                    $"compiler error.",
                    tokens[0].m_sourceOrigin
                );
            }

            KeywordState state = QueryKeywordState(tokens[1]);

            if (state == KeywordState.SUPPORTED)
            {
                // If we get here, then we want to start the token stream at a
                // later position. We do not want to parse the # character again.
                tokens.position++;

                InternalParseResult result = DispatchKeywordParser(tokens[0], tokens);

                if (result.status == EInternalParseStatus.SUCCESS && result.node != null)
                {
                    return result.node;
                }
            }
            else
            {
                if (IsParsingInDuiXmlFile())
                {
                    throw new ParseError(
                        $"Unsupported preprocessor command \"{tokens[1].ToSafeString()}\" " +
                        $"in DUIXML file",
                        tokens[1].m_sourceOrigin
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
                "if"      => KeywordState.SUPPORTED,
                "ifdef"   => KeywordState.SUPPORTED,
                "ifndef"  => KeywordState.SUPPORTED,
                "elif"    => KeywordState.SUPPORTED,
                "else"    => KeywordState.SUPPORTED,
                "endif"   => KeywordState.SUPPORTED,
                "define"  => KeywordState.SUPPORTED,
                "undef"   => KeywordState.SUPPORTED,
                "include" => KeywordState.SUPPORTED,
                "pragma"  => KeywordState.SUPPORTED,
                "error"   => KeywordState.SUPPORTED,
                 
                "import"  => KeywordState.UNSUPPORTED,
                "line"    => KeywordState.UNSUPPORTED,
                "using"   => KeywordState.UNSUPPORTED,

                _ => KeywordState.INVALID
            };
        }

        protected bool IsParsingInDuiXmlFile()
        {
            if (m_sourceFile is SourceFile f && f.GetFileType() == SourceFile.FileType.DUI_UIFILE)
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

        protected InternalParseResult ParseIf(TokenStream tokenStream)
        {
            Debug.Assert(tokenStream[0].ToString().ToLower() == "if");

            return new InternalParseResult(EInternalParseStatus.SUCCESS, null);
        }

        protected InternalParseResult ParseIfDef(TokenStream tokenStream)
        {
            Token initialToken = tokenStream[0];

            Debug.Assert(initialToken.ToString().ToLower() == "ifdef");

            Token nameToken = tokenStream.Next();

            IfNode ifNode = new(initialToken.m_sourceOrigin);

            ParseNode definedCheck = new("DefinedCheck", nameToken.m_sourceOrigin);
            definedCheck.SetAttribute("Name", nameToken.ToSafeString());

            ifNode.Expression.AppendChild(definedCheck);

            return new InternalParseResult(
                EInternalParseStatus.SUCCESS,
                ifNode
            );
        }

        protected InternalParseResult ParseIfNDef(TokenStream tokenStream)
        {
            return new InternalParseResult(EInternalParseStatus.SUCCESS, null);
        }

        protected InternalParseResult ParseElIf(TokenStream tokenStream)
        {
            return new InternalParseResult(EInternalParseStatus.SUCCESS, null);
        }
        protected InternalParseResult ParseElse(TokenStream tokenStream)
        {
            return new InternalParseResult(EInternalParseStatus.SUCCESS, null);
        }

        protected InternalParseResult ParseEndIf(TokenStream tokenStream)
        {
            return new InternalParseResult(EInternalParseStatus.SUCCESS, null);
        }
        protected InternalParseResult ParseDefine(TokenStream tokenStream)
        {
            return new InternalParseResult(EInternalParseStatus.SUCCESS, null);
        }

        protected InternalParseResult ParseUndef(TokenStream tokenStream)
        {
            return new InternalParseResult(EInternalParseStatus.SUCCESS, null);
        }

        protected InternalParseResult ParseInclude(TokenStream tokenStream)
        {
            return new InternalParseResult(EInternalParseStatus.SUCCESS, null);
        }

        protected InternalParseResult ParsePragma(TokenStream tokenStream)
        {
            return new InternalParseResult(EInternalParseStatus.SUCCESS, null);
        }

        protected InternalParseResult ParseErrorCommand(TokenStream tokenStream)
        {
            return new InternalParseResult(EInternalParseStatus.SUCCESS, null);
        }

        /// <summary>
        /// Parse a preprocessor expression (i.e. in an #if condition)
        /// </summary>
        protected InternalParseResult ParseExpression(TokenStream tokenStream)
        {


            return new InternalParseResult(EInternalParseStatus.SUCCESS, null);
        }
    }
}
