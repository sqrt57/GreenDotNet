module Green.Interpreter

open Green.Read

type Interpreter() =

    let mainModule : IModule = ReadonlyModule(name = "main",
                                              globals = readOnlyDict[
                                                  "+", Types.GreenFunction(BaseLibrary.Add) :> obj;
                                              ]) :> IModule

    member this.EvalSource(source : string) : obj =
        let mutable result = null
        for expr in read source do
            result <- this.Eval(expr)
        result

    member this.Eval(expr : ISyntax) : obj =
        let bytecode = Compile.Compile expr
        Evaluator.Eval(mainModule, bytecode)
