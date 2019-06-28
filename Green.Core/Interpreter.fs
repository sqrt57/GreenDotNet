namespace Green

open Read
open Compile
open Bytecode
open Obj
open Module

module Interpreter =

    type Interpreter() =

        let mainModule:GreenModule = GreenModule.empty "main"
        do GreenModule.tryImport mainModule "+" Prelude.``module`` "+" |> ignore
        do GreenModule.tryImport mainModule "=" Prelude.``module`` "=" |> ignore

        member __.Eval expr : Value =
            match compile expr with
            | None -> failwith "Some error in compiler"
            | Some block -> eval mainModule block

        member this.EvalSource (source:string) : Value =
            match read source with
            | UnexpectedEof -> raise (ReadError "Unexpected end of file")
            | UnexpectedRightBr -> raise (ReadError "Unexpected right bracket")
            | Success syntax -> List.fold (fun _ s -> this.Eval s) Value.empty syntax
