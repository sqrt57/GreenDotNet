namespace Green

open Obj

module Base =

    let add args =
        let addNext sum (x:Value) =
            match x with
            | Int i -> sum + (int64 i)
            | _ -> raise (RuntimeException(sprintf "+: bad argument %A" x))
        Array.fold addNext 0L args |> Value.ofInt
