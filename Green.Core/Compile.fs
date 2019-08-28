namespace Green

open Obj
open Maybe
open Source.Parse
open Bytecode

module Compile =

    type CodeBlock<'con,'var> =
        | Const of 'con
        | VarRef of 'var
        | Call of func : CodeBlock<'con,'var> * args : CodeBlock<'con,'var> list
        | If of cond : CodeBlock<'con,'var> * then_ : CodeBlock<'con,'var> * else_ : CodeBlock<'con,'var>

    module CodeBlock =

        type Cata<'con,'var,'result> =
            abstract member cConst : info:'con -> 'result
            abstract member cVarRef : info:'var -> 'result
            abstract member cCall : func:'result -> args:'result list -> 'result
            abstract member cIf : cond:'result -> then_:'result -> elseBlock:'result -> 'result

        let rec cata (c:Cata<'a,'b,'r>) (block:CodeBlock<'a,'b>) : 'r =
            let recurse = cata c
            match block with
            | Const x -> c.cConst x
            | VarRef x -> c.cVarRef x
            | Call (func=funcBlock; args=argsBlocks) ->
                let func = recurse funcBlock
                let args = List.map recurse argsBlocks
                c.cCall func args
            | If (cond=cond; then_=then_; else_=elseBlock) ->
                c.cIf (recurse cond) (recurse then_) (recurse elseBlock)

        type CataFold<'con,'var,'acc,'result> =
            abstract member fConst : acc:'acc -> info:'con -> 'acc * 'result
            abstract member fVarRef : acc:'acc -> info:'var -> 'acc * 'result
            abstract member fCall : acc:'acc -> func:'result -> args:'result list -> 'acc * 'result
            abstract member fIf : acc:'acc -> cond:'result -> then_:'result -> elseBlock:'result -> 'acc * 'result

        let rec cataFold (f : CataFold<'a,'b,'acc,'result>) (acc : 'acc) (block : CodeBlock<'a,'b>) : 'acc * 'result =

            let recurse = cataFold f

            match block with
            | Const x -> f.fConst acc x
            | VarRef x -> f.fVarRef acc x
            | Call (func=funcBlock; args=argBlocks) ->
                let argFolder (acc,argsAcc) arg =
                    let acc,arg = recurse acc arg
                    (acc,arg::argsAcc)

                let acc,func = recurse acc funcBlock
                let acc,argsAcc = List.fold argFolder (acc,[]) argBlocks
                let args = List.rev argsAcc
                f.fCall acc func args

            | If (cond=condBlock; then_=thenBlock; else_=elseBlock) ->
                let acc,cond = recurse acc condBlock
                let acc,then_ = recurse acc thenBlock
                let acc,else_ = recurse acc elseBlock
                f.fIf acc cond then_ else_

    let rec syntaxToBlocks ({syntax=syntax} : 't SyntaxWithInfo) : CodeBlock<Value,string> option =
        match syntax with
        | Syntax.Constant value -> Some <| Const value
        | Syntax.Identifier name -> Some <| VarRef name
        | Syntax.List [] -> None
        | Syntax.List (func::args) -> maybe {
            let! funcBlock = syntaxToBlocks func
            let! argsBlocks = List.traverseOption syntaxToBlocks args
            return Call (func=funcBlock, args=argsBlocks)
        }

    let extractConstants (block:CodeBlock<Value,'a>) : CodeBlock<int,'a> * Value seq =
        let folder = {
            new CodeBlock.CataFold<Value, 'a, int * Value list, CodeBlock<int,'a>> with
                member __.fConst ((num, objAcc)) constant = (num+1, constant::objAcc), Const num
                member __.fVarRef acc info = acc, VarRef info
                member __.fCall acc func args = acc, Call (func=func,args=args)
                member __.fIf acc cond then_ else_ = acc, If (cond=cond,then_=then_,else_=else_) }
        let (_num,constsAcc),result = CodeBlock.cataFold folder (0,[]) block
        result, (constsAcc |> List.rev |> List.toSeq)

    let extractVariables (block:CodeBlock<'a,string>) : CodeBlock<'a,int> * string seq =
        let folder = {
            new CodeBlock.CataFold<'a, string, int * string list, CodeBlock<'a,int>> with
                member __.fConst acc info = acc, Const info
                member __.fVarRef ((num,nameAcc)) name = (num+1, name::nameAcc), VarRef num
                member __.fCall acc func args = acc, Call (func=func,args=args)
                member __.fIf acc cond then_ else_ = acc, If (cond=cond,then_=then_,else_=else_) }
        let (_num,constsAcc),result = CodeBlock.cataFold folder (0,[]) block
        result, (constsAcc |> List.rev |> List.toSeq)

    let extractBytecode (block:CodeBlock<int,int>) : byte seq =
        let c = {
            new CodeBlock.Cata<int, int, byte seq> with
                member __.cConst index = seq [Checked.byte OpCode.Const1; Checked.byte index]
                member __.cVarRef index = seq [Checked.byte OpCode.Var1; Checked.byte index]
                member __.cCall func args =
                    seq {
                        yield! func
                        for arg in args do
                            yield! arg
                        yield Checked.byte OpCode.Call1
                        yield Checked.byte <| List.length args
                    }
                member __.cIf cond thenBlock elseBlock =
                    let t = List.ofSeq thenBlock
                    let e = List.ofSeq elseBlock
                    seq {
                        yield! cond
                        yield Checked.byte OpCode.JumpIfNot1
                        yield Checked.byte <| List.length t
                        yield! t
                        yield Checked.byte OpCode.Jump1
                        yield Checked.byte <| List.length e
                        yield! e
                    }
        }
        CodeBlock.cata c block

    let compile (expr: 'T SyntaxWithInfo) : Block option =
        maybe {
            let! block = syntaxToBlocks expr
            let block,constants = extractConstants block
            let block,variables = extractVariables block
            let bytecode = extractBytecode block
            return BlockCreate.toBlock {bytecode=bytecode; constants=constants; variables=variables}
        }
