namespace Green
  module Compile = begin
    type CodeBlock<'con,'var> =
      | Const of 'con
      | VarRef of 'var
      | Call of func: CodeBlock<'con,'var> * args: CodeBlock<'con,'var> list
      | If of
        cond: CodeBlock<'con,'var> * then_: CodeBlock<'con,'var> *
        else_: CodeBlock<'con,'var>
    module CodeBlock = begin
      type Cata<'con,'var,'result> =
        interface
          abstract member cCall : func:'result -> args:'result list -> 'result
          abstract member cConst : info:'con -> 'result
          abstract member
            cIf : cond:'result -> then_:'result -> elseBlock:'result -> 'result
          abstract member cVarRef : info:'var -> 'result
        end
      val cata : c:Cata<'a,'b,'r> -> block:CodeBlock<'a,'b> -> 'r
      type CataFold<'con,'var,'acc,'result> =
        interface
          abstract member
            fCall : acc:'acc ->
                      func:'result -> args:'result list -> 'acc * 'result
          abstract member fConst : acc:'acc -> info:'con -> 'acc * 'result
          abstract member
            fIf : acc:'acc ->
                    cond:'result ->
                      then_:'result -> elseBlock:'result -> 'acc * 'result
          abstract member fVarRef : acc:'acc -> info:'var -> 'acc * 'result
        end
      val cataFold :
        f:CataFold<'a,'b,'acc,'result> ->
          acc:'acc -> block:CodeBlock<'a,'b> -> 'acc * 'result
    end
    val syntaxToBlocks :
      't Source.Parse.SyntaxWithInfo -> CodeBlock<Obj.Value,string> option
    val extractConstants :
      block:CodeBlock<Obj.Value,'a> -> CodeBlock<int,'a> * seq<Obj.Value>
    val extractVariables :
      block:CodeBlock<'a,string> -> CodeBlock<'a,int> * seq<string>
    val extractBytecode : block:CodeBlock<int,int> -> seq<byte>
    val compile : expr:'T Source.Parse.SyntaxWithInfo -> Bytecode.Block option
  end
