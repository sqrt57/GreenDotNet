module EvaluatorTests

open Xunit
open Green
open Bytecode
open Module
open Obj

[<Fact>]
let ``Eval constant``() =
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
let ``Eval variable``() =
    let main:Module = {
        name="main";
        bindings = Map.ofList [
            "x", 5L |> Value.ofInt |> Cell.cell |> Binding.value
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
let ``Eval call``() =
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

[<Fact>]
let ``Eval jump``() =
    let main:Module = {
        name="main";
        bindings = Map.empty;
    }
    let bytecode = BlockCreate.toBlock {
        bytecode=[
            byte OpCode.Jump1; 1uy
            0uy
            byte OpCode.Const1; 0uy

        ]
        constants=[Value.ofInt 4L]
        variables=[]
    }
    let result = eval main bytecode

    Assert.Equal(Value.ofInt 4L, result)

[<Fact>]
let ``Eval JumpIfNot when false condition then jumps``() =
    let main:Module = {
        name="main";
        bindings = Map.empty;
    }
    let bytecode = BlockCreate.toBlock {
        bytecode=[
            byte OpCode.Const1; 0uy
            byte OpCode.JumpIfNot1; 1uy
            0uy
            byte OpCode.Const1; 1uy
        ]
        constants=[Value.ofBool false; Value.ofInt 5L]
        variables=[]
    }
    let result = eval main bytecode

    Assert.Equal(Value.ofInt 5L, result)

[<Fact>]
let ``Eval JumpIfNot when true condition then does not jump``() =
    let main:Module = {
        name="main";
        bindings = Map.empty;
    }
    let bytecode = BlockCreate.toBlock {
        bytecode=[
            byte OpCode.Const1; 0uy
            byte OpCode.JumpIfNot1; 2uy
            byte OpCode.Const1; 1uy
        ]
        constants=[Value.ofBool true; Value.ofInt 5L]
        variables=[]
    }
    let result = eval main bytecode

    Assert.Equal(Value.ofInt 5L, result)
