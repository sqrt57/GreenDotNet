using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Green
{
    public class Reader
    {
        public IEnumerable<ISyntax> Read(string source)
        {
            var enumerator = Scan(source)
                .Select(t =>
                {
                    var (lexeme, syntaxInfo) = t;
                    var (type, value, name) = EvalToken(lexeme);
                    return (type, value, name, syntaxInfo);
                })
                .GetEnumerator();
            return ReadList(enumerator, innerList: false);
        }

        private IEnumerable<ISyntax> ReadList(
            IEnumerator<(TokenType type, object value, string name, SyntaxInfo syntaxInfo)> enumerator,
            bool innerList)
        {
            while (enumerator.MoveNext())
            {
                var (type, value, name, syntaxInfo) = enumerator.Current;
                switch (type)
                {
                    case TokenType.LeftBracket:
                        var sublist = new ReadOnlyCollection<ISyntax>(ReadList(enumerator, true).ToArray());
                        var listSyntaxInfo = syntaxInfo;
                        yield return new SyntaxList(listSyntaxInfo, sublist);
                        break;
                    case TokenType.RightBracket:
                        if (innerList)
                            yield break;
                        else
                            throw new ReaderException("read: unexpected right bracket");
                    case TokenType.Number:
                        yield return new SyntaxConstant(syntaxInfo, value);
                        break;
                    case TokenType.Identifier:
                        yield return new SyntaxIdentifier(syntaxInfo, IdentifierType.Identifier, name);
                        break;
                }
            }
        }

        public IEnumerable<(string lexeme, SyntaxInfo syntaxInfo)> Scan(string source)
        {
            var sourceInfo = new SourceInfo(SourceType.String, null);
            var positionBuilder = new SourcePositionBuilder();
            SourcePosition atomPosition = positionBuilder.Current;
            var enumerator = LookAhead.Enumerate(source);

            while (enumerator.HasNext())
            {
                char c = enumerator.Next();
                enumerator.Advance();

                var position = positionBuilder.Current;
                positionBuilder.Update(c);

                if (!Char.IsWhiteSpace(c))
                {
                    if (c == '(')
                        yield return ("(", SyntaxInfo.FromBeginSpan(sourceInfo, position, 1));
                    else if (c == ')')
                        yield return (")", SyntaxInfo.FromBeginSpan(sourceInfo, position, 1));
                    else
                    {
                        atomPosition = position;
                        var atom = new StringBuilder();
                        atom.Append(c);
                        while (enumerator.HasNext())
                        {
                            char n = enumerator.Next();
                            if (n == '(' || n == ')' || Char.IsWhiteSpace(n))
                                break;
                            atom.Append(n);
                            positionBuilder.Update(n);
                            enumerator.Advance();
                        }
                        yield return (atom.ToString(), SyntaxInfo.FromBeginEnd(sourceInfo, atomPosition, positionBuilder.Current));
                    }
                }
            }
        }

        public (TokenType type, object value, string name) EvalToken(string lexeme)
        {
            if (lexeme == "(")
                return (TokenType.LeftBracket, default, default);
            if (lexeme == ")")
                return (TokenType.RightBracket, default, default);
            if (Int64.TryParse(lexeme, out var number))
                return (TokenType.Number, number, default);
            return (TokenType.Identifier, default, lexeme);
        }
    }
}
