module InterpreterTests

open Xunit
open Green

[<Fact>]
let Eval_Number() =
    let interpreter = Interpreter()
    let result = interpreter.EvalSource("10")
    Assert.Equal<obj>(10L, result)

[<Fact>]
let Eval_Add() =
    let interpreter = Interpreter()
    let result = interpreter.EvalSource("(+ 2 3)")
    Assert.Equal<obj>(5L, result)

[<Fact>]
let Eval_NestedAdd() =
    let interpreter = Interpreter()
    let result = interpreter.EvalSource("(+ (+) (+ 2 3) (+ 4 5 6))")
    Assert.Equal<obj>(20L, result)
