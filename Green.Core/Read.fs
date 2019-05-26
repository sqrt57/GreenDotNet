namespace Green

open System
open System.Collections.Generic
open System.Linq
open System.Text
open Source
open Source.Range
open Source.Lex
open Source.Parse

module Read =

    let evalToken (lexeme:string) : Token =
        let mutable number = 0L
        match lexeme with
        | "(" -> LeftBracket
        | ")" -> RightBracket
        | _ when Int64.TryParse(lexeme, &number) -> Number number
        | _ -> Token.Identifier lexeme

    let rec private scanSymbol pos (enumerator:LookAheadEnumerator<char>) (name:StringBuilder) =
        if enumerator.HasNext() then
            let c = enumerator.Next()
            match c with
            | '(' | ')' -> pos
            | _ when Char.IsWhiteSpace(c) -> pos
            | _ ->
                name.Append(c) |> ignore
                enumerator.Advance()
                scanSymbol (Position.update c pos) enumerator name
        else
            pos

    let rec private scanFromPos pos (enumerator:LookAheadEnumerator<char>) : struct (string*Range) seq =
        seq {
            if enumerator.HasNext() then
                let c = enumerator.Next()
                enumerator.Advance();
                let nextPos = Position.update c pos

                match c with
                | _ when Char.IsWhiteSpace(c) ->
                    yield! scanFromPos nextPos enumerator
                | '(' ->
                    yield "(", Range.fromPosPos pos nextPos
                    yield! scanFromPos nextPos enumerator
                | ')' ->
                    yield ")", Range.fromPosPos pos nextPos
                    yield! scanFromPos nextPos enumerator
                | _ ->
                    let atom = StringBuilder()
                    atom.Append(c) |> ignore
                    let endPos = scanSymbol nextPos enumerator atom
                    yield atom.ToString(), Range.fromPosPos pos endPos
                    yield! scanFromPos endPos enumerator
        }

    let scan (source:string) : struct (string*Range) seq =
        let enumerator = LookAhead.Enumerate(source)
        scanFromPos Position.zero enumerator

    let rec private readList
            (enumerator : LookAheadEnumerator<struct (Token*Range)>)
            (innerList : bool)
            : Range SyntaxWithInfo seq =
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

                    let listSyntaxInfo = Range.combine syntaxInfo rightSyntaxInfo
                    yield {info=listSyntaxInfo; syntax=Syntax.List sublist}

                | RightBracket ->
                    if innerList then
                        working <- false
                    else
                        raise (ReaderException("read: unexpected right bracket"))

                | Number value ->
                    enumerator.Advance()
                    yield {info=syntaxInfo; syntax=Syntax.Constant value}

                | Token.Identifier name ->
                    enumerator.Advance()
                    yield {info=syntaxInfo; syntax=Syntax.Identifier name}
        }

    let read (source : string) : Range SyntaxWithInfo seq =
        let enumerator = LookAhead.Enumerate(
                            (scan source).Select(fun struct (lexeme, syntaxInfo) ->
                                let token = evalToken lexeme
                                struct (token, syntaxInfo)
                            ))
        readList enumerator false

    type InteractiveReadResult = Range SyntaxWithInfo list option

    let readInteractive(lines : IReadOnlyList<string>) : InteractiveReadResult =
        try
            String.Join("\n", lines) |> read |> Seq.toList |> Some
        with
        | :? ReaderUnexpectedEof -> None
