module CompileTests

open Xunit
open Green
open Green.Source.Parse
open Green.Compile

[<Fact>]
let Compile_Constant() =
    let expr = {info=null; syntax=Constant 5L}
    let result = compile expr

    Assert.Equal([byte OpCode.Const1; byte 0], result.Code)
    Assert.Equal<obj>([ 5L ], result.Constants)

[<Fact>]
let Compile_Variable() =
    let expr = {info=null; syntax=Identifier "a"}
    let result = compile expr

    Assert.Equal([byte OpCode.Var1; byte 0], result.Code)
    Assert.Equal(["a"], result.Variables)

[<Fact>]
let Compile_Call() =
    let expr = {info=null; syntax=List [
        {info=null; syntax=Identifier "+"};
        {info=null; syntax=Constant 1};
        {info=null; syntax=Constant 2};
    ]}
    let result = compile expr

    Assert.Equal([
            byte OpCode.Var1; byte 0;
            byte OpCode.Const1; byte 0;
            byte OpCode.Const1; byte 1;
            byte OpCode.Call1; byte 2;
        ],
        result.Code)
    Assert.Equal<obj>([1; 2], result.Constants)
    Assert.Equal(["+"], result.Variables)
