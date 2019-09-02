namespace Green

type RuntimeException(message) = inherit System.Exception(message)

module Obj =

    [<StructuralEquality;StructuralComparison>]
    type Value =
        | Unit
        | Bool of bool
        | Int of int64
        | U of UValue

    and [<Struct;CustomEquality;CustomComparison>]
        UValue =
        | Fun of (Value array -> Value)

        override __.Equals _ = false
        override __.GetHashCode() = 0
        interface System.IComparable with
            member __.CompareTo _ = failwith "not comparable"

    let (|Fun|_|) x =
        match x with
        | U (Fun f) -> Some f
        | _ -> None

    module Value =

        let empty = Unit

        let ofInt x = Int x

        let ofBool x = Bool x

        let ofFun f = U <| Fun f

        let isUnit = function
            | Unit -> true
            | _ -> false
