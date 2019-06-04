namespace Green

open Read
open Compile

module Interpreter =

    type Interpreter() =

        let mainModule : IModule = ReadonlyModule(name = "main",
                                                  globals = readOnlyDict[
                                                      "+", Types.GreenFunction(BaseLibrary.Add) :> obj;
                                                  ]) :> IModule

        member this.Eval expr : obj =
            let bytecode = compile expr
            Evaluator.Eval(mainModule, bytecode)

        member this.EvalSource(source : string) : obj =
            match read source with
            | UnexpectedEof -> raise (ReadError "Unexpected end of file")
            | UnexpectedRightBr -> raise (ReadError "Unexpected right bracket")
            | Success syntax ->
                let mutable result = null
                for expr in syntax do
                    result <- this.Eval expr
                result
