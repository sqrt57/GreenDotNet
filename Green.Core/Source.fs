namespace Green

open Obj
open System

module Source =

    type SourceType = File | String

    type SourceInfo =
        | File of string
        | UserInput
        | String

    [<Struct>]
    type Position = {pos:int; line:int; col:int}

    module Position =

        let zero = {pos=0; line=0; col=0}

        let update char {pos=pos; line=line; col=col} =
            match char with
            | '\n' -> {pos = pos + 1; line = line + 1; col = 0}
            | _ -> {pos = pos + 1; line = line; col = col + 1}


    [<Struct>]
    type Range = {left:Position; right:Position}

    module Range =

        let fromPosPos left right = {left=left; right=right}

        let combine {left=left} {right=right} = {left=left; right=right}

    module Lex =

        type Token =
            | LeftBracket
            | RightBracket
            | Number of Int64
            | Boolean of bool
            | Identifier of string

    module Parse =

        type 'T SyntaxWithInfo = {info: 'T; syntax: 'T Syntax}

        and 'T Syntax =
            | List of 'T SyntaxWithInfo list
            | Constant of Value
            | Identifier of string
