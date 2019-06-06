namespace Green

open Read
open Compile
open Bytecode

module Interpreter =

    type Interpreter() =

        let mainModule : IModule = ReadonlyModule(name = "main",
                                                  globals = readOnlyDict[
                                                      "+", Types.GreenFunction(BaseLibrary.Add) :> obj;
                                                  ]) :> IModule

        member this.Eval expr : obj =
            let block = compile expr
            eval mainModule block

        member this.EvalSource (source:string) : obj =
            match read source with
            | UnexpectedEof -> raise (ReadError "Unexpected end of file")
            | UnexpectedRightBr -> raise (ReadError "Unexpected right bracket")
            | Success syntax -> List.fold (fun _ s -> this.Eval s) null syntax
