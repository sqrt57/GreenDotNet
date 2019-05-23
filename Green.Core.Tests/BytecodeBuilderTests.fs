module BytecodeBuilderTests

open Xunit
open Green

[<Fact>]
let AddConstant() =
    let builder = BytecodeBuilder()
    let index1 = builder.AddConstant(10)
    let index2 = builder.AddConstant(20)
    let result = builder.ToBytecode()

    Assert.Equal(0, index1)
    Assert.Equal(1, index2)
    Assert.Equal<obj>([ 10 ; 20 ], result.Constants)

[<Fact>]
let AddVariable() =
    let builder = BytecodeBuilder()
    let index1 = builder.AddVariable("a")
    let index2 = builder.AddVariable("b")
    let result = builder.ToBytecode()

    Assert.Equal(0, index1)
    Assert.Equal(1, index2)
    Assert.Equal([ "a" ; "b" ], result.Variables)
