namespace Green

open Read
open Compile
open Bytecode

module Interpreter =

    type Interpreter() =

        let mainModule:Module = {
            name="main";
            globals = Map.ofList [
                "+", Base.add :> obj;
            ]}

        member this.Eval expr : obj =
            match compile expr with
            | None -> "Some error in compiler" :> obj
            | Some block -> eval mainModule block

        member this.EvalSource (source:string) : obj =
            match read source with
            | UnexpectedEof -> raise (ReadError "Unexpected end of file")
            | UnexpectedRightBr -> raise (ReadError "Unexpected right bracket")
            | Success syntax -> List.fold (fun _ s -> this.Eval s) null syntax
