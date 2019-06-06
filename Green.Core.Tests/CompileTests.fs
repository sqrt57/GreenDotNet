module CompileTests

open Xunit
open Green
open Green.Source.Parse
open Green.Compile
open Green.Bytecode

[<Fact>]
let Compile_Constant() =
    let expr = {info=null; syntax=Constant 5L}
    let block = BlockCreate.ofBlock <| compile expr
    Assert.Equal([|byte OpCode.Const1; byte 0|], block.bytecode)
    Assert.Equal<obj>([|5L|], block.constants)

[<Fact>]
let Compile_Variable() =
    let expr = {info=null; syntax=Identifier "a"}
    let block = BlockCreate.ofBlock <| compile expr
    Assert.Equal([|byte OpCode.Var1; byte 0|], block.bytecode)
    Assert.Equal([|"a"|], block.variables)

[<Fact>]
let Compile_Call() =
    let expr = {info=null; syntax=List [
        {info=null; syntax=Identifier "+"};
        {info=null; syntax=Constant 1};
        {info=null; syntax=Constant 2};
    ]}
    let block = BlockCreate.ofBlock <| compile expr

    Assert.Equal([|
            byte OpCode.Var1; byte 0;
            byte OpCode.Const1; byte 0;
            byte OpCode.Const1; byte 1;
            byte OpCode.Call1; byte 2;
        |],
        block.bytecode)
    Assert.Equal<obj>([|1; 2|], block.constants)
    Assert.Equal([|"+"|], block.variables)
