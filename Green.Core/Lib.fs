namespace Green

open Obj
open Module

module Prelude =

    let add args =
        let addNext sum (x:Value) =
            match x with
            | Int i -> sum + (int64 i)
            | _ -> raise (RuntimeException(sprintf "+: bad argument %A" x))
        Array.fold addNext 0L args |> Value.ofInt

    let equals = function
        | [|Int x; Int y|] -> Value.ofBool (x = y)
        | [|_;_|] -> raise (RuntimeException "=: wrong type of arguments")
        | _ -> raise (RuntimeException "=: wrong number of arguments")

    let ``module`` : Module = {
        name="lib"
        bindings = Map.ofList [
            "+", add |> Value.ofFun |> Cell.cell
            "=", equals |> Value.ofFun |> Cell.cell
        ]}
