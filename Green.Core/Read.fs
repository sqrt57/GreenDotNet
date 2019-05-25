namespace Green

open System
open System.Collections.Generic
open System.Linq

module Read =

    let evalToken (lexeme : string) : struct (TokenType * obj * string) =
        let mutable number = 0L
        match lexeme with
        | "(" -> (TokenType.LeftBracket, null, null)
        | ")" -> (TokenType.RightBracket, null, null)
        | _ when Int64.TryParse(lexeme, &number) -> (TokenType.Number, number :> obj, null)
        | _ -> (TokenType.Identifier, null, lexeme)

    let read (source : string) : IEnumerable<ISyntax> =
        let reader = Reader()
        let enumerator = LookAhead.Enumerate(
                            reader.Scan(source).Select(fun struct (lexeme, syntaxInfo) ->
                                let struct (tokenType, value, name) = evalToken lexeme
                                struct (tokenType, value, name, syntaxInfo)
                            ))
        reader.ReadList(enumerator, innerList = false)

    type InteractiveReadResult = ISyntax list option

    let readInteractive(lines : IReadOnlyList<string>) : InteractiveReadResult =
        try
            String.Join("\n", lines) |> read |> Seq.toList |> Some
        with
        | :? ReaderUnexpectedEof -> None
