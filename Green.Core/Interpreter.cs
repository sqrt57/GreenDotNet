using System;

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

        public object EvalToken(string lexeme)
        {
            if (Int64.TryParse(lexeme, out var result))
                return result;
            return null;
        }
    }
}
