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
            let mutable result = null
            for expr in read source do
                result <- this.Eval expr
            result
