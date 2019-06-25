namespace Green

type RuntimeException(message) = inherit System.Exception(message)

module Obj =

    [<StructuralEquality;StructuralComparison>]
    type Value =
        | Unit
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

        let ofFun f = U <| Fun f

        let isUnit = function
            | Unit -> true
            | _ -> false

module Module =

    open Obj

    type ICell<'a> =
        abstract member value: 'a

    type Cell<'a> =
        { value: 'a }
        interface ICell<'a> with
            member this.value with get() = this.value

    module Cell =
        let cell x = { value = x } :> 'a ICell

    type MutableCell<'a> =
        { mutable value: 'a }
        interface ICell<'a> with
            member this.value with get() = this.value

    type IModule =
        abstract member name: string
        abstract member tryGetBinding: name:string -> Value ICell option

    type Module =
        { name: string
          bindings: Map<string, Value ICell> }
        interface IModule with
            member this.name with get() = this.name
            member this.tryGetBinding name = Map.tryFind name this.bindings

    type GreenCell =
        | Define of cell: MutableCell<Value>
        | Import of source: IModule * cell: ICell<Value>

    type GreenModule =
        private {
            name: string
            mutable bindings: Map<string, GreenCell> }
        interface IModule with
            member this.name with get() = this.name
            member this.tryGetBinding name =
                match Map.tryFind name this.bindings with
                | None -> None
                | Some (Define cell) -> cell :> Value ICell |> Some
                | Some (Import (_, cell)) -> cell |> Some

    module GreenModule =

        let empty name = { GreenModule.name = name; bindings = Map.empty }

        type ImportError = | SourceNameNotFound | TargetNameAlreadyExists

        let tryImport (target: GreenModule) (targetName: string) (source: IModule) (sourceName: string)
                : Result<GreenCell,ImportError> =
            match source.tryGetBinding sourceName with
            | None -> Error SourceNameNotFound
            | Some cell ->
                if Map.containsKey targetName target.bindings then
                    Error TargetNameAlreadyExists
                else
                    let newCell = Import (source, cell)
                    target.bindings <- Map.add targetName newCell target.bindings
                    Ok newCell

        let tryGetDefine (name: string) (greenModule: GreenModule) : MutableCell<Value> option =
            match Map.tryFind name greenModule.bindings with
            | None -> None
            | Some (Define cell) -> Some cell
            | Some (Import _) -> None

        let tryGetValue (name: string) (greenModule: GreenModule) : Value option =
            match Map.tryFind name greenModule.bindings with
            | None -> None
            | Some (Define cell) -> Some cell.value
            | Some (Import (_, cell)) -> Some cell.value

        let trySetValue (name: string) (greenModule: GreenModule) (value: Value) : bool =
            match tryGetDefine name greenModule with
            | None -> false
            | Some cell ->
                cell.value <- value
                true

        let tryAddBinding (name: string) (greenModule: GreenModule) (value: Value) : MutableCell<Value> option =
            if Map.containsKey name greenModule.bindings then
                None
            else
                let newCell = { MutableCell.value = value }
                greenModule.bindings <- Map.add name (Define newCell) greenModule.bindings
                Some newCell
