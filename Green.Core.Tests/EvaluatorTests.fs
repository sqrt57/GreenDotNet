module EvaluatorTests

open Xunit
open Green
open Bytecode

[<Fact>]
let Eval_Constant() =
    let main:Module = {
        name="main";
        globals = Map.empty;
    }
    let bytecode = BlockCreate.toBlock {
        bytecode=[byte OpCode.Const1; byte 0;];
        constants=[3];
        variables=[];
    }
    let result = eval main bytecode

    Assert.Equal<obj>(3, result)

[<Fact>]
let Eval_Variable() =
    let main:Module = {
        name="main";
        globals = Map.ofList [
            "x", 5 :> obj;
        ]
    }
    let bytecode = BlockCreate.toBlock {
        bytecode=[byte OpCode.Var1; byte 0;];
        constants=[];
        variables=["x"];
    }
    let result = eval main bytecode

    Assert.Equal<obj>(5, result)

[<Fact>]
let Eval_Call() =
    let main:Module = {
        name="main";
        globals = Map.empty
    }
    let bytecode = BlockCreate.toBlock {
        bytecode=[
            byte OpCode.Const1; byte 0;
            byte OpCode.Const1; byte 1;
            byte OpCode.Const1; byte 2;
            byte OpCode.Call1; byte 2;
        ];
        constants=[Base.add :> obj; 2L :> obj; 3L :> obj;];
        variables=[];
    }
    let result = eval main bytecode

    Assert.Equal<obj>(5L, result)
