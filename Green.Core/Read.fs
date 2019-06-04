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

    module private Scan =

        open Source.Position

        let rec private scanImpl acc source =
            match source with
            | [] -> List.rev acc
            | (c,_)::rest when Char.IsWhiteSpace(c) -> scanImpl acc rest
            | ('(',range)::rest -> scanImpl (("(",range)::acc) rest
            | (')',range)::rest -> scanImpl ((")",range)::acc) rest
            | (_,{left=left;right=right})::_ -> scanAtom acc source [] left right

        and private scanAtom acc source atomAcc left right =
            let getAtom() = atomAcc |> List.rev |> List.toArray |> System.String
            let newAcc() = (getAtom(), {left=left;right=right})::acc
            let retToImpl() = scanImpl (newAcc()) source
            match source with
            | [] -> retToImpl()
            | (c,_)::_ when Char.IsWhiteSpace(c) -> retToImpl()
            | ('(',_)::_ | (')',_)::_ -> retToImpl()
            | (c,{right=newRight})::rest -> scanAtom acc rest (c::atomAcc) left newRight

        let private updatePos (_,{right=prev}) c = (c,{left=prev;right=update c prev})

        let scan (source:string) : (string*Range) list =
            source
            |> Seq.scan updatePos ('\000',{left=zero;right=zero})
            |> Seq.skip 1
            |> Seq.toList
            |> scanImpl []

    let scan = Scan.scan

    let evalToken (lexeme:string) : Token =
        let mutable number = 0L
        match lexeme with
        | "(" -> LeftBracket
        | ")" -> RightBracket
        | _ when Int64.TryParse(lexeme, &number) -> Number number
        | _ -> Token.Identifier lexeme

    let rec private readList
            (enumerator : LookAheadEnumerator<struct (Token*Range)>)
            (innerList : bool)
            : SyntaxWithInfo<Range> seq =
        seq {
            if enumerator.HasNext() then
                let struct (tokenType, syntaxInfo) = enumerator.Next()
                match tokenType with
                | LeftBracket ->
                    enumerator.Advance();
                    let sublist = readList enumerator true |> Seq.toList

                    if not (enumerator.HasNext()) then
                        raise (ReaderUnexpectedEof("read: unexpected eof while reading list"))

                    let struct (shouldBeRightBracket, rightSyntaxInfo) = enumerator.Next()
                    if shouldBeRightBracket <> RightBracket then
                        raise (ReaderException("read: list should end with right bracket"))
                    enumerator.Advance()

                    let listSyntaxInfo = Range.combine syntaxInfo rightSyntaxInfo
                    yield {info=listSyntaxInfo; syntax=Syntax.List sublist}

                    yield! readList enumerator innerList

                | RightBracket ->
                    if not innerList then
                        raise (ReaderException("read: unexpected right bracket"))

                | Number value ->
                    enumerator.Advance()
                    yield {info=syntaxInfo; syntax=Syntax.Constant value}

                    yield! readList enumerator innerList

                | Token.Identifier name ->
                    enumerator.Advance()
                    yield {info=syntaxInfo; syntax=Syntax.Identifier name}

                    yield! readList enumerator innerList
        }

    let read (source : string) : Range SyntaxWithInfo seq =
        let enumerator = LookAhead.Enumerate(
                            (scan source).Select(fun (lexeme, syntaxInfo) ->
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
