namespace Green
  module Source = begin
    type SourceType =
      | File
      | String
    type SourceInfo =
      | File of string
      | UserInput
      | String
    [<StructAttribute ()>]
    type Position =
      {pos: int;
       line: int;
       col: int;}
    module Position = begin
      val zero : Position
      val update : char:char -> Position -> Position
    end
    [<StructAttribute ()>]
    type Range =
      {left: Position;
       right: Position;}
    module Range = begin
      val fromPosPos : left:Position -> right:Position -> Range
      val combine : Range -> Range -> Range
    end
    module Lex = begin
      type Token =
        | LeftBracket
        | RightBracket
        | Number of System.Int64
        | Boolean of bool
        | Identifier of string
    end
    module Parse = begin
      type 'T SyntaxWithInfo =
        {info: 'T;
         syntax: 'T Syntax;}
      and 'T Syntax =
        | List of 'T SyntaxWithInfo list
        | Constant of Obj.Value
        | Identifier of string
    end
  end
