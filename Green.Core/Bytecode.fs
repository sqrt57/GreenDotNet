namespace Green

open Obj
open Module

module Bytecode =

    type OpCode =
        | Const1 = 0uy
        | Var1 = 1uy
        | Call1 = 2uy
        | Jump1 = 3uy
        | JumpIfNot1 = 4uy

    type Block = private {
        bytecode : byte array
        constants : Value array
        variables : string array
    }

    type BlockCreate = {
        bytecode : byte seq
        constants : Value seq
        variables : string seq
    }

    module BlockCreate =
        let toBlock ({bytecode=bytecode;constants=constants;variables=variables}:BlockCreate) : Block =
            {bytecode=Seq.toArray bytecode;constants=Seq.toArray constants;variables=Seq.toArray variables}
        let ofBlock ({bytecode=bytecode;constants=constants;variables=variables}:Block) : BlockCreate =
            {bytecode=Array.copy bytecode;constants=Array.copy constants;variables=Array.copy variables}

    let eval (main:IModule) (bytecode:Block) =
        let stack = System.Collections.Generic.Stack<Value>()
        let mutable ip = 0
        while ip < Array.length bytecode.bytecode do
            let op : OpCode = LanguagePrimitives.EnumOfValue bytecode.bytecode.[ip]
            ip <- ip + 1
            match op with
            | OpCode.Const1 ->
                let index = bytecode.bytecode.[ip]
                ip <- ip + 1
                let value = bytecode.constants.[int index]
                stack.Push value
            | OpCode.Var1 ->
                let index = bytecode.bytecode.[ip]
                ip <- ip + 1
                let variable = bytecode.variables.[int index]
                match main.tryGetBinding variable with
                | None -> raise (RuntimeException(sprintf "eval: Variable '%s' not found in globals" variable))
                | Some { bindingType=Value; cell=cell } -> stack.Push cell.value
                | Some { bindingType=Syntax } -> raise (RuntimeException(sprintf "eval: Variable '%s' is bound to syntax" variable))
            | OpCode.Call1 ->
                let argsCount = bytecode.bytecode.[ip]
                ip <- ip + 1
                let args = Array.create<Value> (int argsCount) Value.empty
                for i = (int argsCount) - 1 downto 0 do
                    args.[i] <- stack.Pop()
                match stack.Pop() with
                | Fun func ->
                    stack.Push <| func args
                | _ -> raise (RuntimeException <| sprintf "Function required")
            | OpCode.Jump1 ->
                let delta = bytecode.bytecode.[ip]
                ip <- ip + 1 + int delta
            | OpCode.JumpIfNot1 ->
                match stack.Pop() with
                | Bool true ->
                    ip <- ip + 1
                | Bool false ->
                    let delta = bytecode.bytecode.[ip]
                    ip <- ip + 1 + int delta
                | _ -> raise (RuntimeException <| sprintf "Boolean required")
            | _ -> raise (RuntimeException(sprintf "eval: unknown bytecode %A" op))

        if stack.Count = 0 then
            raise (RuntimeException("eval: No result on stack at the end of evaluation"))
        if stack.Count > 1 then
            raise (RuntimeException("eval: Multiple results on stack at the end of evaluation"))

        stack.Pop()
