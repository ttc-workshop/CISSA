using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intersoft.CISSA.DataAccessLayer.Utils
{
    public enum TokenType
    {
        Eof,
        Symbol,
        Ident,
        String,
        Char,
        Number,
        Operation,
        Comment,
        LineComment
    }

    public class CsParser
    {
        public string Script { get; private set; }

        public int StartPos { get; private set; }
        public int EndPos { get; private set; }

        private TokenType _token;
        public TokenType Token
        {
            get
            {
                if (EndPos == 0 && _token != TokenType.Eof) NextToken();
                return _token;
            }
            private set { _token = value; }
        }

        public CsParser(string script)
        {
            Script = script;
            StartPos = 0;
            EndPos = 0;
            if (String.IsNullOrEmpty(Script)) Token = TokenType.Eof;
        }

        public string TokenSymbol
        {
            get
            {
                string ret = "";
                for (int i = StartPos; i < EndPos; i++) ret += Script[i];
                return ret;
            }
        }

        public string GetIdent()
        {
            if (NextToken() != TokenType.Ident)
                throw new Exception(String.Format("Identifier expected but {0} found", TokenSymbol));

            return TokenSymbol;
        }

        public void GetSymbol(string symbol)
        {
            NextToken();

            if (symbol != TokenSymbol) throw new Exception(String.Format("\"{0}\" expected but \"{1}\" found", symbol, TokenSymbol));
        }

        public TokenType NextToken()
        {
            if (Script.Length <= EndPos) Token = TokenType.Eof;
            else
            {
                SkipSpaces();

                if (Script.Length <= EndPos)
                {
                    Token = TokenType.Eof;
                    return Token;
                }
                    
                StartPos = EndPos;

                var ch = Script[EndPos];

                if ((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') || ch == '_')
                {
                    EndPos++;
                    Token = ParseIdent();
                } 
                else if (ch >= '0' && ch <= '9')
                {
                    EndPos++;
                    Token = ParseNumber();
                }
                else if (ch == '/')
                {
                    EndPos++;
                    if (Script[EndPos] == '*') Token = ParseComment();
                    else if (Script[EndPos] == '/') Token = ParseCommentLine();
                    else
                        Token = TokenType.Symbol;
                }
                else if (ch == '"')
                {
                    EndPos++;
                    Token = ParseString();
                }
                else if (ch == '\'')
                {
                    EndPos++;
                    Token = ParseChar();
                }
                else if (ch == '@')
                {
                    EndPos++;
                    if (Script.Length > EndPos && Script[EndPos] == '"')
                        Token = ParseString();
                    else
                        Token = TokenType.Symbol;
                }
                else if (ch == '+')
                {
                    EndPos++;
                    Token = TokenType.Operation;
                    if (Script.Length > EndPos &&
                        (Script[EndPos] == '+' || Script[EndPos] == '=')) EndPos++;
                }
                else if (ch == '-')
                {
                    EndPos++;
                    Token = TokenType.Operation;
                    if (Script.Length > EndPos && 
                        (Script[EndPos] == '-' || Script[EndPos] == '=')) EndPos++;
                }
                else if (ch == '*')
                {
                    EndPos++;
                    Token = TokenType.Operation;
                    if (Script.Length > EndPos && Script[EndPos] == '=') EndPos++;
                }
                else if (ch == '>')
                {
                    EndPos++;
                    Token = TokenType.Operation;
                    if (Script.Length > EndPos &&
                        (Script[EndPos] == '=' || Script[EndPos] == '>')) EndPos++;
                }
                else if (ch == '<')
                {
                    EndPos++;
                    Token = TokenType.Operation;
                    if (Script.Length > EndPos &&
                        (Script[EndPos] == '=' || Script[EndPos] == '<')) EndPos++;
                }
                else if (ch == '=')
                {
                    EndPos++;
                    Token = TokenType.Operation;
                    if (Script.Length > EndPos &&
                        (Script[EndPos] == '=' || Script[EndPos] == '>')) EndPos++;
                }
                else if (ch == '!')
                {
                    EndPos++;
                    if (Script.Length > EndPos && Script[EndPos] == '=')
                    {
                        Token = TokenType.Operation;
                        EndPos++;
                    }
                    else
                        Token = TokenType.Symbol;
                }
                else if (ch == '&')
                {
                    EndPos++;
                    Token = TokenType.Operation;
                    if (Script.Length > EndPos &&
                        (Script[EndPos] == '=' || Script[EndPos] == '&')) EndPos++;
                }
                else if (ch == '|')
                {
                    EndPos++;
                    Token = TokenType.Operation;
                    if (Script.Length > EndPos &&
                        (Script[EndPos] == '=' || Script[EndPos] == '|')) EndPos++;
                }
                else if (ch == '%')
                {
                    EndPos++;
                    Token = TokenType.Operation;
                }
                else
                {
                    EndPos++;
                    Token = TokenType.Symbol;
                }
            }
            return Token;
        }

        public void SkipComments()
        {
            var token = NextToken();

            if (token == TokenType.Comment || token == TokenType.LineComment) SkipComments();
        }

        private void SkipSpaces()
        {
            while (Script.Length > EndPos)
            {
                var ch = Script[EndPos];

                if (ch == ' ' || ch == (char)9 || ch == (char)13 || ch == (char)10)
                    EndPos++;
                else
                    break;
            }
        }

        private TokenType ParseIdent()
        {
            while (EndPos < Script.Length)
            {
                var ch = Script[EndPos];

                if ((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') || ch == '_')
                    EndPos++;
                else
                    break;
            }

            return TokenType.Ident;
        }

        private TokenType ParseNumber()
        {
            while (EndPos < Script.Length)
            {
                var ch = Script[EndPos];

                if ((ch >= '0' && ch <= '9') || ch == '.')
                    EndPos++;
                else
                    break;
            }

            return TokenType.Number;
        }

        private TokenType ParseComment()
        {
            while (EndPos < Script.Length)
            {
                var ch = Script[EndPos];

                if (ch == '*')
                {
                    EndPos++;
                    if (Script.Length > EndPos && Script[EndPos] == '/')
                    {
                        EndPos++;
                        break;
                    }
                }
                EndPos++;
            }

            return TokenType.Comment;
        }

        private TokenType ParseCommentLine()
        {
            while (EndPos < Script.Length)
            {
                var ch = Script[EndPos];

                if (ch == (char)10)
                {
                    EndPos++;
                    break;
                }
                EndPos++;
            }

            return TokenType.LineComment;
        }

        private TokenType ParseString()
        {
            while (EndPos < Script.Length)
            {
                var ch = Script[EndPos];

                if (ch == '\\') EndPos += 2;
                if (ch != '"') continue;
                EndPos++;
                break;
            }

            return TokenType.String;
        }

        private TokenType ParseChar()
        {
            while (EndPos < Script.Length)
            {
                var ch = Script[EndPos];

                if (ch == '\\') EndPos += 2;
                if (ch != '\'') continue;
                EndPos++;
                break;
            }

            return TokenType.Char;
        }
    }
}
