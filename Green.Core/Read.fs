namespace Green

open System
open System.Collections.Generic
open System.Linq
open System.Collections.ObjectModel
open System.Text

module Read =

    let evalToken (lexeme : string) : struct (TokenType * obj * string) =
        let mutable number = 0L
        match lexeme with
        | "(" -> (TokenType.LeftBracket, null, null)
        | ")" -> (TokenType.RightBracket, null, null)
        | _ when Int64.TryParse(lexeme, &number) -> (TokenType.Number, number :> obj, null)
        | _ -> (TokenType.Identifier, null, lexeme)

    let scan (source : string) : IEnumerable<struct (string * SyntaxInfo)> =
        let sourceInfo = new SourceInfo(SourceType.String, null)
        let positionBuilder = new SourcePositionBuilder()
        let enumerator = LookAhead.Enumerate(source)

        seq {
            while enumerator.HasNext() do
                let c = enumerator.Next()
                enumerator.Advance();

                let position = positionBuilder.Current
                positionBuilder.Update(c)

                match c with
                | _ when Char.IsWhiteSpace(c) -> ()
                | '(' -> yield struct ("(", SyntaxInfo.FromBeginSpan(sourceInfo, position, 1))
                | ')' -> yield struct (")", SyntaxInfo.FromBeginSpan(sourceInfo, position, 1))
                | _ ->
                    let atomPosition = position
                    let atom = StringBuilder()
                    atom.Append(c) |> ignore
                    let mutable working = true
                    while enumerator.HasNext() && working do
                        let n = enumerator.Next()
                        match n with
                        | '(' | ')' -> working <- false
                        | _ when Char.IsWhiteSpace(n) -> working <- false
                        | _ ->
                            atom.Append(n) |> ignore
                            positionBuilder.Update(n)
                            enumerator.Advance()
                    yield struct (atom.ToString(), SyntaxInfo.FromBeginEnd(sourceInfo, atomPosition, positionBuilder.Current))
        }

    let rec readList
            (enumerator : LookAheadEnumerator<struct (TokenType * obj * string * SyntaxInfo)>)
            (innerList : bool)
            : ISyntax seq =
        seq {
            let mutable working = true
            while enumerator.HasNext() && working do
                let struct (tokenType, value, name, syntaxInfo) = enumerator.Next()
                match tokenType with
                | TokenType.LeftBracket ->
                    enumerator.Advance();
                    let sublist = ReadOnlyCollection<ISyntax>((readList enumerator true).ToArray())

                    if not (enumerator.HasNext()) then
                        raise (ReaderUnexpectedEof("read: unexpected eof while reading list"))

                    let struct (rightBracketType, _, _, rightSyntaxInfo) = enumerator.Next()
                    enumerator.Advance()
                    if rightBracketType <> TokenType.RightBracket then
                        raise (ReaderException("read: list should end with right bracket"))

                    let listSyntaxInfo = SyntaxInfo.FromLeftRight(syntaxInfo, rightSyntaxInfo);
                    yield SyntaxList(listSyntaxInfo, sublist)

                | TokenType.RightBracket ->
                    if innerList then
                        working <- false
                    else
                        raise (ReaderException("read: unexpected right bracket"))

                | TokenType.Number ->
                    enumerator.Advance()
                    yield SyntaxConstant(syntaxInfo, value)

                | TokenType.Identifier ->
                    enumerator.Advance()
                    yield SyntaxIdentifier(syntaxInfo, IdentifierType.Identifier, name)

                | _ -> ()
        }

    let read (source : string) : IEnumerable<ISyntax> =
        let enumerator = LookAhead.Enumerate(
                            (scan source).Select(fun struct (lexeme, syntaxInfo) ->
                                let struct (tokenType, value, name) = evalToken lexeme
                                struct (tokenType, value, name, syntaxInfo)
                            ))
        readList enumerator false

    type InteractiveReadResult = ISyntax list option

    let readInteractive(lines : IReadOnlyList<string>) : InteractiveReadResult =
        try
            String.Join("\n", lines) |> read |> Seq.toList |> Some
        with
        | :? ReaderUnexpectedEof -> None
