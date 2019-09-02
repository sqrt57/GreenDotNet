namespace Green
  module Read = begin
    type ReadError =
      class
        inherit System.Exception
        new : message:string -> ReadError
      end
    type 'x ReadResult =
      | Success of 'x
      | UnexpectedEof
      | UnexpectedRightBr
    val scan : (string -> (string * Source.Range) list)
    val evalToken : lexeme:string -> Source.Lex.Token
    val readList :
      ((Source.Lex.Token * Source.Range) list ->
         Source.Range Source.Parse.SyntaxWithInfo list ReadResult)
    val read :
      source:string -> Source.Range Source.Parse.SyntaxWithInfo list ReadResult
    type InteractiveReadResult =
      Source.Range Source.Parse.SyntaxWithInfo list option
    val readInteractive :
      lines:System.Collections.Generic.IReadOnlyList<string> ->
        InteractiveReadResult
  end
