module SourceTests

open Xunit
open Green.Source
open Green.Source.Position

[<Fact>]
let SourcePosition_Initial_ZeroPosition() =
    let pos = zero
    Assert.Equal({pos=0; line=0; col=0}, pos)

[<Fact>]
let SourcePositionBuilder_OneLine() =
    let pos = zero |> update 'q' |> update 'w'
    Assert.Equal({pos=2; line=0; col=2}, pos)

[<Fact>]
let SourcePositionBuilder_SecondLine() =
    let pos = zero |> update 'q' |> update '\n' |> update 'w'
    Assert.Equal({pos=3; line=1; col=1}, pos)
