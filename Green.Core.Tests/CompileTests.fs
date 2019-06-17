module CompileTests

open Xunit
open Green
open Green.Source.Parse
open Green.Compile
open Green.Bytecode
open Green.Obj

[<Fact>]
let Compile_Constant() =
    let expr = {info=null; syntax=Constant <| Value.ofInt 5L}
    match compile expr with
    | None -> Assert.True(false, "Should return Some block")
    | Some block ->
        let block = BlockCreate.ofBlock block
        Assert.Equal([|byte OpCode.Const1; byte 0|], block.bytecode)
        Assert.Equal([|Value.ofInt 5L|], block.constants)

[<Fact>]
let Compile_Variable() =
    let expr = {info=null; syntax=Identifier "a"}
    match compile expr with
    | None -> Assert.True(false, "Should return Some block")
    | Some block ->
        let block = BlockCreate.ofBlock block
        Assert.Equal([|byte OpCode.Var1; byte 0|], block.bytecode)
        Assert.Equal([|"a"|], block.variables)

[<Fact>]
let Compile_Call() =
    let expr = {info=null; syntax=List [
        {info=null; syntax=Identifier "+"};
        {info=null; syntax=Constant <| Value.ofInt 1L};
        {info=null; syntax=Constant <| Value.ofInt 2L};
    ]}
    match compile expr with
    | None -> Assert.True(false, "Should return Some block")
    | Some block ->
        let block = BlockCreate.ofBlock block

        Assert.Equal([|
                byte OpCode.Var1; byte 0;
                byte OpCode.Const1; byte 0;
                byte OpCode.Const1; byte 1;
                byte OpCode.Call1; byte 2;
            |],
            block.bytecode)
        Assert.Equal([|Value.ofInt 1L; Value.ofInt 2L|], block.constants)
        Assert.Equal([|"+"|], block.variables)
