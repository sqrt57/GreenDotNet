
namespace Green
  module Maybe = begin
    type MaybeBuilder =
      class
        new : unit -> MaybeBuilder
        member Bind : x:'b option * f:('b -> 'c option) -> 'c option
        member Return : x:'a -> 'a option
      end
    val maybe : MaybeBuilder
  end
  module List = begin
    val traverseOption : f:('a -> 'b option) -> list:'a list -> 'b list option
  end
