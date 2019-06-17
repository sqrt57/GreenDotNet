module InterpreterTests

open Xunit
open Green.Interpreter
open Green.Obj

[<Fact>]
let Eval_Number() =
    let interpreter = Interpreter()
    let result = interpreter.EvalSource("10")
    Assert.Equal(Value.ofInt 10L, result)

[<Fact>]
let Eval_Add() =
    let interpreter = Interpreter()
    let result = interpreter.EvalSource("(+ 2 3)")
    Assert.Equal(Value.ofInt 5L, result)

[<Fact>]
let Eval_NestedAdd() =
    let interpreter = Interpreter()
    let result = interpreter.EvalSource("(+ (+) (+ 2 3) (+ 4 5 6))")
    Assert.Equal(Value.ofInt 20L, result)
