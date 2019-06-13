module UtilTests

open Green
open Xunit

[<Fact>]
let ``traverse empty list``() =
    let f x = Some x
    let result = List.traverseOption f []
    Assert.Equal(result, Some [])

[<Fact>]
let ``traverse Some``() =
    let f x = match x with | 1 -> Some 10 | 2 -> Some 20 | _ -> None
    let result = List.traverseOption f [1;2]
    Assert.Equal(result, Some [10;20])

[<Fact>]
let ``traverse None``() =
    let f x = match x with | 1 -> None | 2 -> Some 20 | _ -> None
    let result = List.traverseOption f [1;2]
    Assert.Equal(result, None)
