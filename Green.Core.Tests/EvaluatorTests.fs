module EvaluatorTests

open Xunit
open Green
open Bytecode
open Module
open Obj

[<Fact>]
let Eval_Constant() =
    let main:Module = {
        name="main";
        bindings = Map.empty;
    }
    let bytecode = BlockCreate.toBlock {
        bytecode=[byte OpCode.Const1; byte 0;];
        constants=[Value.ofInt 3L];
        variables=[];
    }
    let result = eval main bytecode

    Assert.Equal(Value.ofInt 3L, result)

[<Fact>]
let Eval_Variable() =
    let main:Module = {
        name="main";
        bindings = Map.ofList [
            "x", 5L |> Value.ofInt |> Cell.cell
        ]
    }
    let bytecode = BlockCreate.toBlock {
        bytecode=[byte OpCode.Var1; byte 0;];
        constants=[];
        variables=["x"];
    }
    let result = eval main bytecode

    Assert.Equal(Value.ofInt 5L, result)

[<Fact>]
let Eval_Call() =
    let main:Module = {
        name="main";
        bindings = Map.empty
    }
    let bytecode = BlockCreate.toBlock {
        bytecode=[
            byte OpCode.Const1; byte 0;
            byte OpCode.Const1; byte 1;
            byte OpCode.Const1; byte 2;
            byte OpCode.Call1; byte 2;
        ];
        constants=[Value.ofFun Prelude.add; Value.ofInt 2L; Value.ofInt 3L;];
        variables=[];
    }
    let result = eval main bytecode

    Assert.Equal(Value.ofInt 5L, result)
