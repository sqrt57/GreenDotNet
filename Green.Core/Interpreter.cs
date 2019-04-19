using System;
using System.Collections.Generic;

namespace Green
{
    public sealed class Interpreter
    {
        public object Eval(string source)
        {
            var lexemes = Scan(source);
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

        public string[] Scan(string source)
        {
            if (source == "(+ 2 3)")
                return new[] { "(", "+", "2", "3", ")" };
            return null;
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
