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
                        var listSyntaxInfo = new SyntaxInfo(
                            syntaxInfo.Source,
                            syntaxInfo.Position,
                            syntaxInfo.Span,
                            syntaxInfo.LineNumber,
                            syntaxInfo.ColumnNumber);
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
            bool inAtom = false;
            var atom = new StringBuilder();
            int position = 0, line = 0, column = 0;
            int atomPosition = 0, atomLine = 0, atomColumn = 0;

            foreach (char c in source)
            {
                if (inAtom)
                {
                    if (Char.IsWhiteSpace(c))
                    {
                        yield return (atom.ToString(),
                            new SyntaxInfo(sourceInfo, atomPosition, position - atomPosition, atomLine, atomColumn));
                        inAtom = false;
                    }
                    else if (c == '(')
                    {
                        yield return (atom.ToString(),
                            new SyntaxInfo(sourceInfo, atomPosition, position - atomPosition, atomLine, atomColumn));
                        inAtom = false;
                        yield return ("(", new SyntaxInfo(sourceInfo, position, 1, line, column));
                    }
                    else if (c == ')')
                    {
                        yield return (atom.ToString(),
                            new SyntaxInfo(sourceInfo, atomPosition, position - atomPosition, atomLine, atomColumn));
                        inAtom = false;
                        yield return (")", new SyntaxInfo(sourceInfo, position, 1, line, column));
                    }
                    else
                        atom.Append(c);
                }
                else
                {
                    if (!Char.IsWhiteSpace(c))
                    {
                        if (c == '(')
                            yield return ("(", new SyntaxInfo(sourceInfo, position, 1, line, column));
                        else if (c == ')')
                            yield return (")", new SyntaxInfo(sourceInfo, position, 1, line, column));
                        else
                        {
                            inAtom = true;
                            atomPosition = position;
                            atomLine = line;
                            atomColumn = column;
                            atom.Clear();
                            atom.Append(c);
                        }
                    }
                }
                position++;
                if (c == '\n')
                {
                    line++;
                    column = 0;
                }
                else
                    column++;
            }

            if (inAtom)
            {
                yield return (atom.ToString(),
                    new SyntaxInfo(sourceInfo, atomPosition, position - atomPosition, atomLine, atomColumn));
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
