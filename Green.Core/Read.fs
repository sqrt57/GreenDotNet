namespace Green

open System
open System.Collections.Generic
open Obj
open Source
open Source.Range
open Source.Lex
open Source.Parse

module Read =

    type ReadError(message) =
        inherit Exception(message)

    type 'x ReadResult =
        | Success of 'x
        | UnexpectedEof
        | UnexpectedRightBr

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

    module private ReadList =

        let rec readListImpl contError contEol contRightBr acc tokens =
            let cont elem rest = readListImpl contError contEol contRightBr (elem::acc) rest
            let subContEol _ = contError()
            let subContRightBr left right subList rest = cont {syntax=Syntax.List subList;info=combine left right} rest
            match tokens with
            | [] -> contEol (List.rev acc)
            | (LeftBracket,info)::rest -> readListImpl contError subContEol (subContRightBr info) [] rest
            | (RightBracket,info)::rest -> contRightBr info (List.rev acc) rest
            | (Token.Number n,info)::rest -> cont {syntax=Constant (Value.ofInt n);info=info} rest
            | (Token.Boolean b,info)::rest -> cont {syntax=Constant (Value.ofBool b);info=info} rest
            | (Token.Identifier s,info)::rest -> cont {syntax=Identifier s;info=info} rest

        let readList (tokens:(Token*Range) list) : SyntaxWithInfo<Range> list ReadResult =
            let contError() = UnexpectedEof
            let contEol list = Success list
            let contRightBr _ _ _ = UnexpectedRightBr
            readListImpl contError contEol contRightBr [] tokens

    let scan = Scan.scan

    let evalToken (lexeme:string) : Token =
        let mutable number = 0L
        match lexeme with
        | "(" -> LeftBracket
        | ")" -> RightBracket
        | "#t" | "#true" -> Boolean true
        | "#f" | "#false" -> Boolean false
        | _ when Int64.TryParse(lexeme, &number) -> Number number
        | _ -> Token.Identifier lexeme

    let readList = ReadList.readList

    let read (source:string) : SyntaxWithInfo<Range> list ReadResult =
        source
        |> scan
        |> List.map (fun (lexeme,info) -> (evalToken lexeme,info))
        |> readList

    type InteractiveReadResult = SyntaxWithInfo<Range> list option

    let readInteractive(lines : IReadOnlyList<string>) : InteractiveReadResult =
        match String.Join("\n", lines) |> read with
        | UnexpectedEof -> None
        | UnexpectedRightBr -> raise (ReadError "Unexpected right bracket")
        | Success result -> Some result
