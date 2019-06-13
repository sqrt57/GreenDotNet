namespace Green

open Maybe
open Source.Parse
open Bytecode

module Compile =

    type CodeBlock<'constInfo,'varInfo> =
        | Const of 'constInfo * obj
        | VarRef of 'varInfo * string
        | Call of func : CodeBlock<'constInfo,'varInfo> * args : CodeBlock<'constInfo,'varInfo> list

    module CodeBlock =

        type Cata<'constInfo,'varInfo,'result> =
            abstract member cConst : info:'constInfo -> constant:obj -> 'result
            abstract member cVarRef : info:'varInfo -> name:string -> 'result
            abstract member cCall : func:'result -> args:'result list -> 'result

        let rec cata (c:Cata<'a,'b,'r>) (block:CodeBlock<'a,'b>) : 'r =
            let recurse = cata c
            match block with
            | Const (x,y) -> c.cConst x y
            | VarRef (x,y) -> c.cVarRef x y
            | Call (func=funcBlock; args=argsBlocks) ->
                let func = recurse funcBlock
                let args = List.map recurse argsBlocks
                c.cCall func args

        type Fold<'constInfo,'varInfo,'acc> =
            abstract member fConst : acc:'acc -> info:'constInfo -> constant:obj -> 'acc
            abstract member fVarRef : acc:'acc -> info:'varInfo -> name:string -> 'acc
            abstract member fPreCall : acc:'acc -> 'acc
            abstract member fPostCallFunc : acc:'acc -> 'acc
            abstract member fPostCallArgs : acc:'acc -> 'acc

        let rec fold (f : Fold<'a,'b,'acc>) (acc : 'acc) (block : CodeBlock<'a,'b>) : 'acc =
            let recurse = fold f
            match block with
            | Const (x,y) -> f.fConst acc x y
            | VarRef (x,y) -> f.fVarRef acc x y
            | Call (func=funcBlock; args=argBlocks) ->
                acc
                |> f.fPreCall
                |> fun acc -> recurse acc funcBlock
                |> f.fPostCallFunc
                |> fun acc -> List.fold recurse acc argBlocks
                |> f.fPostCallArgs

        type CataFold<'constInfo,'varInfo,'acc,'result> =
            abstract member fConst : acc:'acc -> info:'constInfo -> constant:obj -> 'acc * 'result
            abstract member fVarRef : acc:'acc -> info:'varInfo -> name:string -> 'acc * 'result
            abstract member fPreCall : acc:'acc -> 'acc
            abstract member fPostCallFunc : acc:'acc -> func:'result -> 'acc
            abstract member fPostCallArgs : acc:'acc -> func:'result -> args:'result list -> 'acc * 'result

        let rec cataFold (f : CataFold<'a,'b,'acc,'result>) (acc : 'acc) (block : CodeBlock<'a,'b>) : 'acc * 'result =

            let recurse = cataFold f

            match block with
            | Const (x,y) -> f.fConst acc x y
            | VarRef (x,y) -> f.fVarRef acc x y
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

    let rec syntaxToBlocks ({syntax=syntax} : 't SyntaxWithInfo) : CodeBlock<unit,unit> option =
        match syntax with
        | Syntax.Constant value -> Some <| Const ((), value)
        | Syntax.Identifier name -> Some <| VarRef ((), name)
        | Syntax.List [] -> None
        | Syntax.List (func::args) -> maybe {
            let! funcBlock = syntaxToBlocks func
            let! argsBlocks = List.traverseOption syntaxToBlocks args
            return Call (func=funcBlock, args=argsBlocks)
        }

    let extractConstants (combine:'a->int->'a1) (block:CodeBlock<'a,'b>) : CodeBlock<'a1,'b> * obj seq =
        let folder = {
            new CodeBlock.CataFold<'a, 'b, int * obj list, CodeBlock<'a1,'b>> with
                member __.fConst ((num, objAcc)) info constant = (num+1, constant::objAcc), Const (combine info num, obj)
                member __.fVarRef acc info name = acc, VarRef (info, name)
                member __.fPreCall acc = acc
                member __.fPostCallFunc acc _ = acc
                member __.fPostCallArgs acc func args = acc, Call (func=func,args=args) }
        let (_num,constsAcc),result = CodeBlock.cataFold folder (0,[]) block
        result, (constsAcc |> List.rev |> List.toSeq)

    let extractVariables (combine:'b->int->'b1) (block:CodeBlock<'a,'b>) : CodeBlock<'a,'b1> * string seq =
        let folder = {
            new CodeBlock.CataFold<'a, 'b, int * string list, CodeBlock<'a,'b1>> with
                member __.fConst acc info constant = acc, Const (info, constant)
                member __.fVarRef ((num,nameAcc)) info name = (num+1, name::nameAcc), VarRef (combine info num, name)
                member __.fPreCall acc = acc
                member __.fPostCallFunc acc _ = acc
                member __.fPostCallArgs acc func args = acc, Call (func=func,args=args) }
        let (_num,constsAcc),result = CodeBlock.cataFold folder (0,[]) block
        result, (constsAcc |> List.rev |> List.toSeq)

    let extractBytecode (block:CodeBlock<int,int>) : byte seq =
        let c = {
            new CodeBlock.Cata<int, int, byte seq> with
                member __.cConst index _constant = seq [byte OpCode.Const1; byte index]
                member __.cVarRef index _name = seq [byte OpCode.Var1; byte index]
                member __.cCall func args =
                    seq {
                        yield! func
                        for arg in args do
                            yield! arg
                        yield byte OpCode.Call1
                        yield byte <| List.length args
                    } }
        CodeBlock.cata c block

    let compile (expr: 'T SyntaxWithInfo) : Block option =
        maybe {
            let! block = syntaxToBlocks expr
            let block,constants = extractConstants (fun () num -> num) block
            let block,variables = extractVariables (fun () num -> num) block
            let bytecode = extractBytecode block
            return BlockCreate.toBlock {bytecode=bytecode; constants=constants; variables=variables}
        }
