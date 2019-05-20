module Green.Repl

open System
open System.Collections.Generic

let eval (interpreter : Interpreter) objects =
    let results = new List<Object>()

    for o in objects do
        let result = interpreter.Eval o
        match result with
        | null -> ()
        | result -> results.Add result

    for result in results do
        printfn "%A" result

let run _ =
    let interpreter = new Interpreter()
    let reader = new Reader()
    let lines = new List<string>()

    printfn "Green REPL"

    while true do
        match lines.Count with
        | 0 -> printf "Green> "
        | _ -> printf ". "

        try
            lines.Add <| Console.ReadLine()
            let result = reader.ReadInteractive lines
            if result.Finished then
                eval interpreter result.Objects
                lines.Clear()
        with ex ->
            printfn "Error: %s" ex.Message
            lines.Clear()

    0

[<EntryPoint>]
let main argv =
    run argv
