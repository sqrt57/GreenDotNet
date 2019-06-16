namespace Green

open Read
open Compile
open Bytecode
open Module

module Interpreter =

    type Interpreter() =

        let libModule:Module = {
            name="lib"
            bindings = Map.ofList [
                "+", Base.add :> obj |> Cell.cell
            ]}

        let mainModule:GreenModule = GreenModule.empty "main"
        do GreenModule.tryImport mainModule "+" libModule "+" |> ignore

        member this.Eval expr : obj =
            match compile expr with
            | None -> "Some error in compiler" :> obj
            | Some block -> eval mainModule block

        member this.EvalSource (source:string) : obj =
            match read source with
            | UnexpectedEof -> raise (ReadError "Unexpected end of file")
            | UnexpectedRightBr -> raise (ReadError "Unexpected right bracket")
            | Success syntax -> List.fold (fun _ s -> this.Eval s) null syntax
