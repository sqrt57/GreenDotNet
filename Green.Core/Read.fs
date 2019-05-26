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

    let scan (source:string) : struct (string*Range) seq =
        let enumerator = LookAhead.Enumerate(source)
        let mutable pos = Position.zero

        seq {
            while enumerator.HasNext() do
                let c = enumerator.Next()
                enumerator.Advance();

                let left = pos
                pos <- Position.update c pos

                match c with
                | _ when Char.IsWhiteSpace(c) -> ()
                | '(' -> yield "(", Range.fromPosPos left pos
                | ')' -> yield ")", Range.fromPosPos left pos
                | _ ->
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
                            pos <- Position.update n pos
                            enumerator.Advance()
                    yield atom.ToString(), Range.fromPosPos left pos
        }

    let rec readList
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
