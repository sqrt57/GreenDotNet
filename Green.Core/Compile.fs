namespace Green

open Source.Parse

module Compile =

    exception CompileException of string

    type BytecodeBuilder() =

        let bytecode = System.Collections.Generic.List<byte>()
        let constants = System.Collections.Generic.List<obj>()
        let variables = System.Collections.Generic.List<string>()

        member this.AddCode (code:OpCode) = bytecode.Add(byte code)

        member this.AddCode (code:byte) = bytecode.Add(code)

        member this.AddConstant constant =
            constants.Add(constant)
            constants.Count - 1

        member this.AddVariable name =
            variables.Add(name)
            variables.Count - 1

        member this.ToBytecode() =
            Bytecode(code = bytecode.ToArray(),
                     constants = constants.ToArray(),
                     variables = variables.ToArray())

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

    let compile (expr: 'T SyntaxWithInfo) : Bytecode =
        let builder = BytecodeBuilder()
        compileTo builder expr
        builder.ToBytecode()
