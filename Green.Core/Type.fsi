namespace Green
  type RuntimeException =
    class
      inherit System.Exception
      new : message:string -> RuntimeException
    end
  module Obj = begin
    type
        [<StructuralEqualityAttribute (); StructuralComparisonAttribute ()>]
        Value =
      | Unit
      | Bool of bool
      | Int of int64
      | U of UValue
    and
        [<StructAttribute (); CustomEqualityAttribute (); CustomComparisonAttribute ()>]
        UValue =
      | Fun of (Value array -> Value)
      with
        interface System.IComparable
        override Equals : obj -> bool
        override GetHashCode : unit -> int
      end
    val ( |Fun|_| ) : x:Value -> (Value array -> Value) option
    module Value = begin
      val empty : Value
      val ofInt : x:int64 -> Value
      val ofBool : x:bool -> Value
      val ofFun : f:(Value array -> Value) -> Value
      val isUnit : _arg1:Value -> bool
    end
  end
