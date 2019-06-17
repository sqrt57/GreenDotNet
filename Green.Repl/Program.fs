module Green.Repl

open System
open System.Collections.Generic
open Read
open Interpreter
open Obj

let eval (interpreter : Interpreter) objects =
    let results = List<Value>()

    for o in objects do
        let result = interpreter.Eval o
        if not (Value.isUnit result) then
            results.Add result

    for result in results do
        printfn "%A" result

let run _ =
    let interpreter = Interpreter()
    let lines = List<string>()

    printfn "Green REPL"

    while true do
        match lines.Count with
        | 0 -> printf "Green> "
        | _ -> printf ". "

        try
            lines.Add <| Console.ReadLine()
            match readInteractive lines with
            | None -> ()
            | Some objects ->
                eval interpreter objects
                lines.Clear()
        with ex ->
            printfn "Error: %s" ex.Message
            lines.Clear()

    0

[<EntryPoint>]
let main argv =
    run argv
