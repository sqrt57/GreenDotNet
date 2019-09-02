namespace Green
  module Interpreter = begin
    type Interpreter =
      class
        new : unit -> Interpreter
        member Eval : expr:'a Source.Parse.SyntaxWithInfo -> Obj.Value
        member EvalSource : source:string -> Obj.Value
      end
  end
