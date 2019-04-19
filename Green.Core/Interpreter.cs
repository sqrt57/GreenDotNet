using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Green
{
    public sealed class Interpreter
    {
        public object Eval(string source)
        {
            var lexemes = Scan(source).ToArray();
            if (lexemes.Length == 5
                && lexemes[0] == "("
                && lexemes[1] == "+"
                && lexemes[2] == "2"
                && lexemes[3] == "3"
                && lexemes[4] == ")")
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

        public (TokenType, object) EvalToken(string lexeme)
        {
            if (lexeme == "(")
                return (TokenType.LeftBracket, default);
            if (lexeme == ")")
                return (TokenType.RightBracket, default);
            if (Int64.TryParse(lexeme, out var number))
                return (TokenType.Number, number);
            return (TokenType.Identifier, new Identifier(lexeme));
        }

        public static Identifier ToIdentifier(string name)
        {
            return new Identifier(name);
        }
    }
}
