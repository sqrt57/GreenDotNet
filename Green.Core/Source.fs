namespace Green

open System

module Source =

    type Token =
        | LeftBracket
        | RightBracket
        | Number of Int64
        | Identifier of string

    type 'T SyntaxWithInfo = { Info : 'T; Syntax : 'T Syntax } 

    and 'T Syntax =
        | List of 'T SyntaxWithInfo list
        | Constant of obj
        | Identifier of string
