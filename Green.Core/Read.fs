namespace Green

open System
open System.Collections.Generic
open System.Linq
open System.Collections.ObjectModel
open System.Text
open Source

module Read =

    let evalToken (lexeme : string) : Token =
        let mutable number = 0L
        match lexeme with
        | "(" -> LeftBracket
        | ")" -> RightBracket
        | _ when Int64.TryParse(lexeme, &number) -> Number number
        | _ -> Token.Identifier lexeme

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
            (enumerator : LookAheadEnumerator<struct (Token * SyntaxInfo)>)
            (innerList : bool)
            : SyntaxInfo SyntaxWithInfo seq =
        seq {
            let mutable working = true
            while enumerator.HasNext() && working do
                let struct (tokenType, syntaxInfo) = enumerator.Next()
                match tokenType with
                | LeftBracket ->
                    enumerator.Advance();
                    let sublist = readList enumerator true |> Seq.toList

                    if not (enumerator.HasNext()) then
                        raise (ReaderUnexpectedEof("read: unexpected eof while reading list"))

                    let struct (shouldBeRightBracket, rightSyntaxInfo) = enumerator.Next()
                    enumerator.Advance()
                    if shouldBeRightBracket <> RightBracket then
                        raise (ReaderException("read: list should end with right bracket"))

                    let listSyntaxInfo = SyntaxInfo.FromLeftRight(syntaxInfo, rightSyntaxInfo);
                    yield { Info = listSyntaxInfo; Syntax = Syntax.List sublist }

                | RightBracket ->
                    if innerList then
                        working <- false
                    else
                        raise (ReaderException("read: unexpected right bracket"))

                | Number value ->
                    enumerator.Advance()
                    yield { Info = syntaxInfo; Syntax = Syntax.Constant value }

                | Token.Identifier name ->
                    enumerator.Advance()
                    yield { Info = syntaxInfo; Syntax = Syntax.Identifier name }
        }

    let read (source : string) : SyntaxInfo SyntaxWithInfo seq =
        let enumerator = LookAhead.Enumerate(
                            (scan source).Select(fun struct (lexeme, syntaxInfo) ->
                                let token = evalToken lexeme
                                struct (token, syntaxInfo)
                            ))
        readList enumerator false

    type InteractiveReadResult = SyntaxInfo SyntaxWithInfo list option

    let readInteractive(lines : IReadOnlyList<string>) : InteractiveReadResult =
        try
            String.Join("\n", lines) |> read |> Seq.toList |> Some
        with
        | :? ReaderUnexpectedEof -> None
