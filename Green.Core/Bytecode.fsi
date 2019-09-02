namespace Green
  module Bytecode = begin
    type OpCode =
      | Const1 = 0uy
      | Var1 = 1uy
      | Call1 = 2uy
      | Jump1 = 3uy
      | JumpIfNot1 = 4uy
    type Block =
      private {bytecode: byte array;
               constants: Obj.Value array;
               variables: string array;}
    type BlockCreate =
      {bytecode: seq<byte>;
       constants: seq<Obj.Value>;
       variables: seq<string>;}
    module BlockCreate = begin
      val toBlock : BlockCreate -> Block
      val ofBlock : Block -> BlockCreate
    end
    val eval : main:Module.IModule -> bytecode:Block -> Obj.Value
  end
