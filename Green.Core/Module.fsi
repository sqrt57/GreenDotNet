namespace Green
  module Module = begin
    type ICell<'a> =
      interface
        abstract member value : 'a
      end
    type BindingType =
      | Syntax
      | Value
    type Binding<'cell> =
      {bindingType: BindingType;
       cell: 'cell;}
    module Binding = begin
      val syntax : cell:'a -> Binding<'a>
      val value : cell:'a -> Binding<'a>
    end
    type IModule =
      interface
        abstract member name : string
        abstract member
          tryGetBinding : name:string -> Binding<ICell<Obj.Value>> option
      end
    type Cell<'a> =
      {value: 'a;}
      with
        interface ICell<'a>
      end
    module Cell = begin
      val cell : x:'a -> ICell<'a>
    end
    type Module =
      {name: string;
       bindings: Map<string,Binding<ICell<Obj.Value>>>;}
      with
        interface IModule
      end
    type MutableCell<'a> =
      {mutable value: 'a;}
      with
        interface ICell<'a>
      end
    type GreenCell =
      | Define of cell: Binding<MutableCell<Obj.Value>>
      | Import of source: IModule * cell: Binding<ICell<Obj.Value>>
    type GreenModule =
      private {name: string;
               mutable bindings: Map<string,GreenCell>;}
      with
        interface IModule
      end
    module GreenModule = begin
      val empty : name:string -> GreenModule
      type ImportError =
        | SourceNameNotFound
        | TargetNameAlreadyExists
      val tryImport :
        target:GreenModule ->
          targetName:string ->
            source:IModule ->
              sourceName:string -> Result<GreenCell,ImportError>
      val tryGetDefine :
        name:string ->
          greenModule:GreenModule -> Binding<MutableCell<Obj.Value>> option
    end
  end
