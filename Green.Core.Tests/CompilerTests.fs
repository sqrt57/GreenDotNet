module CompilerTests

open Xunit
open Green

[<Fact>]
let Compile_Constant() =
    let expr = SyntaxConstant(null, 5L)
    let result = Compile.Compile expr

    Assert.Equal([ byte OpCode.Const1 ; byte 0 ], result.Code)
    Assert.Equal<obj>([ 5L ], result.Constants)

[<Fact>]
let Compile_Variable() =
    let expr = SyntaxIdentifier(null, IdentifierType.Identifier, "a")
    let result = Compile.Compile expr

    Assert.Equal([ byte OpCode.Var1; byte 0 ], result.Code)
    Assert.Equal([ "a" ], result.Variables)

[<Fact>]
let Compile_Call() =
    let expr = SyntaxList(null,
                          [
                              SyntaxIdentifier(null, IdentifierType.Identifier, "+");
                              SyntaxConstant(null, 1);
                              SyntaxConstant(null, 2);
                          ])
    let result = Compile.Compile expr

    Assert.Equal([
                     byte OpCode.Var1; byte 0;
                     byte OpCode.Const1; byte 0;
                     byte OpCode.Const1; byte 1;
                     byte OpCode.Call1; byte 2;
                 ],
                 result.Code)
    Assert.Equal<obj>([ 1; 2 ], result.Constants)
    Assert.Equal([ "+" ], result.Variables)
