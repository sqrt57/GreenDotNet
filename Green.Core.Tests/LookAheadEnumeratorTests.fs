module LookAheadEnumeratorTests

open System
open Xunit
open Green

[<Fact>]
let Next() =
    let x = LookAhead.Enumerate([ 10; 15 ])
    Assert.Equal(10, x.Next(0))
    Assert.Equal(15, x.Next(1))
    Assert.Throws<InvalidOperationException>(fun () -> x.Next(2) |> ignore) |> ignore
    Assert.True(x.HasNext(0))
    Assert.True(x.HasNext(1))
    Assert.False(x.HasNext(2))

[<Fact>]
let Consume() =
    let x = LookAhead.Enumerate([ 10; 15 ])
    Assert.Equal(10, x.Next(0))
    Assert.Equal(15, x.Next(1))
    Assert.Throws<InvalidOperationException>(fun () -> x.Next(2) |> ignore) |> ignore
    x.Advance(1)
    Assert.Equal(15, x.Next(0))
    Assert.Throws<InvalidOperationException>(fun () -> x.Next(1) |> ignore) |> ignore
    Assert.Throws<InvalidOperationException>(fun () -> x.Advance(2) |> ignore) |> ignore

[<Fact>]
let Advance_NumberZero_ChangesNothing() =
    let x = LookAhead.Enumerate([ 10 ])
    Assert.Equal(10, x.Next())
    Assert.False(x.HasNext(1))
    x.Advance(0)
    Assert.Equal(10, x.Next())
    Assert.False(x.HasNext(1))
    x.Advance(1)
    Assert.False(x.HasNext())
    x.Advance(0)
    Assert.False(x.HasNext())

[<Fact>]
let BigList() =
    let x = LookAhead.Enumerate([ 10; 15; 20 ])
    Assert.Equal(10, x.Next(0))
    Assert.Equal(15, x.Next(1))
    Assert.True(x.HasNext(0))
    Assert.True(x.HasNext(1))

    x.Advance(1)
    Assert.Equal(15, x.Next(0))
    Assert.Equal(20, x.Next(1))
    Assert.True(x.HasNext(0))
    Assert.True(x.HasNext(1))

    x.Advance(1)
    Assert.Equal(20, x.Next(0))
    Assert.Throws<InvalidOperationException>(fun () -> x.Next(1) |> ignore) |> ignore
    Assert.True(x.HasNext(0))
    Assert.False(x.HasNext(1))

    x.Advance(1)
    Assert.Throws<InvalidOperationException>(fun () -> x.Next(0) |> ignore) |> ignore
    Assert.Throws<InvalidOperationException>(fun () -> x.Next(1) |> ignore) |> ignore
    Assert.False(x.HasNext(0))
    Assert.False(x.HasNext(1))
