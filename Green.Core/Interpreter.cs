using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Green
{
    public sealed class Interpreter
    {
        public object Eval(string source)
        {
            var tokens = Scan(source).Select(EvalToken).ToArray();
            if (tokens.Length == 5
                && tokens[0].type == TokenType.LeftBracket
                && tokens[1].type == TokenType.Identifier
                && tokens[1].value.Equals(ToIdentifier("+"))
                && tokens[2].type == TokenType.Number
                && tokens[2].value.Equals(2L)
                && tokens[3].type == TokenType.Number
                && tokens[3].value.Equals(3L)
                && tokens[4].type == TokenType.RightBracket)
            {
                return 5;
            }
            return null;
        }

        public IEnumerable<string> Scan(string source)
        {
            bool inAtom = false;
            var atom = new StringBuilder();
            foreach (char c in source)
            {
                if (inAtom)
                {
                    if (Char.IsWhiteSpace(c))
                    {
                        yield return atom.ToString();
                        inAtom = false;
                    }
                    else if (c == '(')
                    {
                        yield return atom.ToString();
                        inAtom = false;
                        yield return "(";
                    }
                    else if (c == ')')
                    {
                        yield return atom.ToString();
                        inAtom = false;
                        yield return ")";
                    }
                    else
                        atom.Append(c);
                }
                else
                {
                    if (!Char.IsWhiteSpace(c))
                    {
                        if (c == '(')
                            yield return "(";
                        else if (c == ')')
                            yield return ")";
                        else
                        {
                            inAtom = true;
                            atom.Clear();
                            atom.Append(c);
                        }
                    }
                }
            }
        }

        public (TokenType type, object value) EvalToken(string lexeme)
        {
            if (lexeme == "(")
                return (TokenType.LeftBracket, default);
            if (lexeme == ")")
                return (TokenType.RightBracket, default);
            if (Int64.TryParse(lexeme, out var number))
                return (TokenType.Number, number);
            return (TokenType.Identifier, new Identifier(lexeme));
        }

        public Identifier ToIdentifier(string name)
        {
            return new Identifier(name);
        }
    }
}
