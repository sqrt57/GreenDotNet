namespace Green

exception CompileException of string

module Compile =

    type BytecodeBuilder() =

        let bytecode = System.Collections.Generic.List<byte>()
        let constants = System.Collections.Generic.List<obj>()
        let variables = System.Collections.Generic.List<string>()

        member this.AddCode (code : OpCode) = bytecode.Add(byte code)

        member this.AddCode (code : byte) = bytecode.Add(code)

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

    let rec compile (builder : BytecodeBuilder) (expr : ISyntax) =
        match expr with
        | :? SyntaxConstant as constant ->
            let constIndex = builder.AddConstant(constant.Value)
            builder.AddCode(OpCode.Const1)
            builder.AddCode(byte constIndex)

        | :? SyntaxIdentifier as identifier ->
            if identifier.Type <> IdentifierType.Identifier then
                raise <| CompileException (sprintf "compile: cannot compile keyword %s" identifier.Name)
            let varIndex = builder.AddVariable(identifier.Name)
            builder.AddCode(OpCode.Var1)
            builder.AddCode(byte varIndex)

        | :? SyntaxList as list ->
            if list.Items.Count = 0 then
                raise <| CompileException "compile: cannot compile empty application: ()"
            for subExpr in list.Items do
                compile builder subExpr
            builder.AddCode(OpCode.Call1)
            builder.AddCode(byte <| list.Items.Count - 1)

        | _ ->
            raise <| CompileException (sprintf "compile: cannot compile %A" expr)

    let Compile(expr : ISyntax) : Bytecode =
        let builder = BytecodeBuilder()
        compile builder expr
        builder.ToBytecode()
