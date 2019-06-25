namespace Green

module Maybe =

    type MaybeBuilder() =

        member this.Bind(x, f) = Option.bind f x

        member this.Return(x) = Some x

    let maybe = MaybeBuilder()

module List =

    let rec traverseOption (f:'a->'b option) (list:'a list) : 'b list option =

        let rec traverseInternal f list acc =
            match list with
            | [] -> Some <| List.rev acc
            | head::tail ->
                match f head with
                | Some result -> traverseInternal f tail (result::acc)
                | None -> None

        traverseInternal f list []
