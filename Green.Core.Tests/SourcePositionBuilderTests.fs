module SourcePositionBuilderTests

open Xunit
open Green

[<Fact>]
let SourcePositionBuilder_Initial_ZeroPosition() =
    let builder = SourcePositionBuilder()
    Assert.Equal(SourcePosition(0, 0, 0), builder.Current)

[<Fact>]
let SourcePositionBuilder_OneLine() =
    let builder = SourcePositionBuilder()
    builder.Update('q')
    builder.Update('w')
    Assert.Equal(SourcePosition(2, 0, 2), builder.Current)

[<Fact>]
let SourcePositionBuilder_SecondLine() =
    let builder = SourcePositionBuilder()
    builder.Update('q')
    builder.Update('\n')
    builder.Update('w')
    Assert.Equal(SourcePosition(3, 1, 1), builder.Current)

[<Fact>]
let SourcePositionBuilder_TwoCurrents_AreIndependent() =
    let builder = SourcePositionBuilder()
    let position1 = builder.Current
    builder.Update('q')
    let position2 = builder.Current
    Assert.NotEqual(position1, position2)
