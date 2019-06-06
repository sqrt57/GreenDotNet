namespace Green

module Base =

    let add args =
        let addNext sum (x:obj) =
            match x with
            | :? int64 as i -> sum + (int64 i)
            | :? int32 as i -> sum + (int64 i)
            | :? uint64 as i -> sum + (int64 i)
            | :? uint32 as i -> sum + (int64 i)
            | _ -> raise (RuntimeException(sprintf "+: bad argument %A" x))
        Array.fold addNext 0L args :> obj
