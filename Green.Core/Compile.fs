namespace Green

open Source.Parse
open Bytecode

module Compile =

    exception CompileException of string

    type BytecodeBuilder() =

        let bytecode = ResizeArray<byte>()
        let constants = ResizeArray<obj>()
        let variables = ResizeArray<string>()

        member this.AddCode (code:OpCode) = bytecode.Add(byte code)

        member this.AddCode (code:byte) = bytecode.Add(code)

        member this.AddConstant constant =
            constants.Add(constant)
            constants.Count - 1

        member this.AddVariable name =
            variables.Add(name)
            variables.Count - 1

        member this.ToBytecode() =
            BlockCreate.toBlock {bytecode=bytecode; constants=constants; variables=variables}

    let rec compileTo (builder:BytecodeBuilder) {syntax=expr} =
        match expr with
        | Syntax.Constant value ->
            let constIndex = builder.AddConstant(value)
            builder.AddCode(OpCode.Const1)
            builder.AddCode(byte constIndex)

        | Syntax.Identifier name ->
            let varIndex = builder.AddVariable(name)
            builder.AddCode(OpCode.Var1)
            builder.AddCode(byte varIndex)

        | Syntax.List list ->
            if List.isEmpty list then
                raise <| CompileException "compile: cannot compile empty application: ()"
            for subExpr in list do
                compileTo builder subExpr
            builder.AddCode(OpCode.Call1)
            builder.AddCode(byte <| List.length list - 1)

    let compile (expr: 'T SyntaxWithInfo) : Block =
        let builder = BytecodeBuilder()
        compileTo builder expr
        builder.ToBytecode()
