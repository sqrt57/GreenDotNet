module Green.Interpreter

type Interpreter() =

    let reader = Reader()
    let mainModule : IModule = ReadonlyModule(name = "main",
                                              globals = readOnlyDict[
                                                  "+", Types.GreenFunction(BaseLibrary.Add) :> obj;
                                              ]) :> IModule

    member this.EvalSource(source : string) : obj =
        let mutable result = null
        for expr in reader.Read(source) do
            result <- this.Eval(expr)
        result

    member this.Eval(expr : ISyntax) : obj =
        let compiler = Compiler()
        let bytecode = compiler.Compile(expr)
        Evaluator.Eval(mainModule, bytecode)
