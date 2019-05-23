module EvaluatorTests

open Xunit
open Green

[<Fact>]
let Eval_Constant() =
    let ``module`` = ReadonlyModule(name = "main",
                                    globals = readOnlyDict["+", Interpreter.GreenFunction(Interpreter.Add) :> obj])

    let bytecode = Bytecode(code = [ byte OpCode.Const1; byte 0; ],
                            constants = [ 3 ],
                            variables = [])

    let result = Evaluator.Eval(``module``, bytecode)

    Assert.Equal<obj>(3, result)

[<Fact>]
let Eval_Variable() =
    let ``module`` = ReadonlyModule(name = "main",
                                    globals = readOnlyDict["x", 5 :> obj])

    let bytecode = Bytecode(code = [ byte OpCode.Var1; byte 0; ],
                            constants = [],
                            variables = [ "x" ])

    let result = Evaluator.Eval(``module``, bytecode)

    Assert.Equal<obj>(5, result)

[<Fact>]
let Eval_Call() =
    let ``module`` = ReadonlyModule(name = "main",
                                    globals = readOnlyDict["+", Interpreter.GreenFunction(Interpreter.Add) :> obj])

    let bytecode = Bytecode(code = [
                                byte OpCode.Const1; byte 0;
                                byte OpCode.Const1; byte 1;
                                byte OpCode.Const1; byte 2;
                                byte OpCode.Call1; byte 2;
                            ],
                            constants = [ Interpreter.GreenFunction(Interpreter.Add) :> obj; 2L :> obj; 3L :> obj; ],
                            variables = [])

    let result = Evaluator.Eval(``module``, bytecode)

    Assert.Equal<obj>(5L, result)
