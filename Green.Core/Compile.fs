namespace Green

open Obj
open Maybe
open Source.Parse
open Bytecode

module Compile =

    type CodeBlock<'constInfo,'varInfo> =
        | Const of 'constInfo
        | VarRef of 'varInfo
        | Call of func : CodeBlock<'constInfo,'varInfo> * args : CodeBlock<'constInfo,'varInfo> list

    module CodeBlock =

        type Cata<'constInfo,'varInfo,'result> =
            abstract member cConst : info:'constInfo -> 'result
            abstract member cVarRef : info:'varInfo -> 'result
            abstract member cCall : func:'result -> args:'result list -> 'result

        let rec cata (c:Cata<'a,'b,'r>) (block:CodeBlock<'a,'b>) : 'r =
            let recurse = cata c
            match block with
            | Const x -> c.cConst x
            | VarRef x -> c.cVarRef x
            | Call (func=funcBlock; args=argsBlocks) ->
                let func = recurse funcBlock
                let args = List.map recurse argsBlocks
                c.cCall func args

        type Fold<'constInfo,'varInfo,'acc> =
            abstract member fConst : acc:'acc -> info:'constInfo -> 'acc
            abstract member fVarRef : acc:'acc -> info:'varInfo -> 'acc
            abstract member fPreCall : acc:'acc -> 'acc
            abstract member fPostCallFunc : acc:'acc -> 'acc
            abstract member fPostCallArgs : acc:'acc -> 'acc

        let rec fold (f : Fold<'a,'b,'acc>) (acc : 'acc) (block : CodeBlock<'a,'b>) : 'acc =
            let recurse = fold f
            match block with
            | Const x -> f.fConst acc x
            | VarRef x -> f.fVarRef acc x
            | Call (func=funcBlock; args=argBlocks) ->
                acc
                |> f.fPreCall
                |> fun acc -> recurse acc funcBlock
                |> f.fPostCallFunc
                |> fun acc -> List.fold recurse acc argBlocks
                |> f.fPostCallArgs

        type CataFold<'constInfo,'varInfo,'acc,'result> =
            abstract member fConst : acc:'acc -> info:'constInfo -> 'acc * 'result
            abstract member fVarRef : acc:'acc -> info:'varInfo -> 'acc * 'result
            abstract member fPreCall : acc:'acc -> 'acc
            abstract member fPostCallFunc : acc:'acc -> func:'result -> 'acc
            abstract member fPostCallArgs : acc:'acc -> func:'result -> args:'result list -> 'acc * 'result

        let rec cataFold (f : CataFold<'a,'b,'acc,'result>) (acc : 'acc) (block : CodeBlock<'a,'b>) : 'acc * 'result =

            let recurse = cataFold f

            match block with
            | Const x -> f.fConst acc x
            | VarRef x -> f.fVarRef acc x
            | Call (func=funcBlock; args=argBlocks) ->

                let argFolder (acc,argsAcc) arg =
                    let acc,arg = recurse acc arg
                    (acc,arg::argsAcc)

                let acc = f.fPreCall acc
                let acc,func = recurse acc funcBlock
                let acc = f.fPostCallFunc acc func
                let acc,argsAcc = List.fold argFolder (acc,[]) argBlocks
                let args = List.rev argsAcc
                f.fPostCallArgs acc func args

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
                member __.fPreCall acc = acc
                member __.fPostCallFunc acc _ = acc
                member __.fPostCallArgs acc func args = acc, Call (func=func,args=args) }
        let (_num,constsAcc),result = CodeBlock.cataFold folder (0,[]) block
        result, (constsAcc |> List.rev |> List.toSeq)

    let extractVariables (block:CodeBlock<'a,string>) : CodeBlock<'a,int> * string seq =
        let folder = {
            new CodeBlock.CataFold<'a, string, int * string list, CodeBlock<'a,int>> with
                member __.fConst acc info = acc, Const info
                member __.fVarRef ((num,nameAcc)) name = (num+1, name::nameAcc), VarRef num
                member __.fPreCall acc = acc
                member __.fPostCallFunc acc _ = acc
                member __.fPostCallArgs acc func args = acc, Call (func=func,args=args) }
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
                    } }
        CodeBlock.cata c block

    let compile (expr: 'T SyntaxWithInfo) : Block option =
        maybe {
            let! block = syntaxToBlocks expr
            let block,constants = extractConstants block
            let block,variables = extractVariables block
            let bytecode = extractBytecode block
            return BlockCreate.toBlock {bytecode=bytecode; constants=constants; variables=variables}
        }
