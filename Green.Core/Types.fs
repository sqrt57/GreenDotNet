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

module Module =

    open Obj

    type ICell<'a> =
        abstract member value: 'a

    type BindingType = Syntax | Value

    type Binding<'cell> = { bindingType : BindingType; cell: 'cell }

    module Binding =
        let syntax cell = { bindingType=Syntax; cell=cell }
        let value cell = { bindingType=Value; cell=cell }

    type IModule =
        abstract member name: string
        abstract member tryGetBinding: name:string -> Binding<ICell<Value>> option

    type Cell<'a> =
        { value: 'a }
        interface ICell<'a> with
            member this.value with get() = this.value

    module Cell =
        let cell x = { value = x } :> 'a ICell

    type Module =
        { name: string
          bindings: Map<string, Binding<ICell<Value>>> }
        interface IModule with
            member this.name with get() = this.name
            member this.tryGetBinding name = Map.tryFind name this.bindings

    type MutableCell<'a> =
        { mutable value: 'a }
        interface ICell<'a> with
            member this.value with get() = this.value

    type GreenCell =
        | Define of cell: Binding<MutableCell<Value>>
        | Import of source: IModule * cell: Binding<ICell<Value>>

    type GreenModule =
        private {
            name: string
            mutable bindings: Map<string, GreenCell> }
        interface IModule with
            member this.name with get() = this.name
            member this.tryGetBinding name =
                match Map.tryFind name this.bindings with
                | None -> None
                | Some (Define { bindingType = bindingType; cell = cell }) -> Some { bindingType = bindingType; cell = cell :> ICell<Value> }
                | Some (Import (_, cell)) -> Some cell

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

        let tryGetDefine (name: string) (greenModule: GreenModule) : Binding<MutableCell<Value>> option =
            match Map.tryFind name greenModule.bindings with
            | None -> None
            | Some (Define binding) -> Some binding
            | Some (Import _) -> None
