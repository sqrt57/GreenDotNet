module ReaderTests

open System.Linq
open Xunit
open Green

[<Fact>]
let Read_Empty() =
    let reader = Reader()
    let result = reader.Read("").ToArray()
    Assert.Empty(result)

[<Fact>]
let Read_Atom_SourceInfo() =
    let reader = Reader()
    let result = reader.Read("58").ToArray()
    Assert.Equal(SourceType.String, result.[0].SyntaxInfo.Source.Type)
    Assert.Null(result.[0].SyntaxInfo.Source.FileName)

[<Fact>]
let Read_Atom_SyntaxInfo() =
    let reader = Reader()
    let result = reader.Read("58").ToArray()
    Assert.Equal(SourcePosition(0, 0, 0), result.[0].SyntaxInfo.Position)
    Assert.Equal(2, result.[0].SyntaxInfo.Span)

[<Fact>]
let Read_List_SyntaxInfo() =
    let reader = Reader()
    let result = reader.Read("(+ 2 3)").ToArray()
    Assert.Equal(SourcePosition(0, 0, 0), result.[0].SyntaxInfo.Position)
    Assert.Equal(7, result.[0].SyntaxInfo.Span)

[<Fact>]
let Read_Number() =
    let reader = Reader()
    let result = reader.Read("5").ToArray()
    let onlyResult = Assert.Single(result)
    let constant = Assert.IsType<SyntaxConstant>(onlyResult)
    Assert.Equal<obj>(5L, constant.Value)

[<Fact>]
let Read_Symbol() =
    let reader = Reader()
    let result = reader.Read("x").ToArray()
    let onlyResult = Assert.Single(result)
    let identifier = Assert.IsType<SyntaxIdentifier>(onlyResult)
    Assert.Equal("x", identifier.Name)

[<Fact>]
let Read_EmptyList() =
    let reader = Reader()
    let result = reader.Read("()").ToArray()
    let onlyResult = Assert.Single(result)
    let list = Assert.IsType<SyntaxList>(onlyResult)
    Assert.Empty(list.Items)

[<Fact>]
let Read_List() =
    let reader = Reader()
    let result = reader.Read("(+ 2 3)").ToArray()
    let onlyResult = Assert.Single(result)
    let list = Assert.IsType<SyntaxList>(onlyResult)
    Assert.Equal(3, list.Items.Count)
    let subItem0 = Assert.IsType<SyntaxIdentifier>(list.Items.[0])
    Assert.Equal(IdentifierType.Identifier, subItem0.Type)
    Assert.Equal("+", subItem0.Name)
    let subItem1 = Assert.IsType<SyntaxConstant>(list.Items.[1])
    Assert.Equal<obj>(2L, subItem1.Value)
    let subItem2 = Assert.IsType<SyntaxConstant>(list.Items.[2])
    Assert.Equal<obj>(3L, subItem2.Value)

[<Fact>]
let Scan_Empty() =
    let reader = Reader()
    let result = reader.Scan("").ToArray()
    Assert.Empty(result)

[<Fact>]
let Scan_Whitespace() =
    let reader = Reader()
    let result = reader.Scan(" ").ToArray()
    Assert.Empty(result)

[<Fact>]
let Scan_Number_SourceInfo() =
    let reader = Reader()
    let result = reader.Scan("15").ToArray()
    let struct (_, syntaxInfo) = Assert.Single(result)
    Assert.Equal(SourceType.String, syntaxInfo.Source.Type)
    Assert.Null(syntaxInfo.Source.FileName)

[<Fact>]
let Scan_Number_SourcePosition() =
    let reader = Reader()
    let result = reader.Scan("15").ToArray()
    let struct (_, syntaxInfo) = Assert.Single(result)
    Assert.Equal(SourcePosition(0, 0, 0), syntaxInfo.Position)
    Assert.Equal(2, syntaxInfo.Span)

[<Fact>]
let Scan_Number() =
    let reader = Reader()
    let result = reader.Scan("15").ToArray()
    let struct (lexeme, _) = Assert.Single(result)
    Assert.Equal("15", lexeme)

[<Fact>]
let Scan_Identifier() =
    let reader = Reader()
    let result = reader.Scan("abc").ToArray()
    let struct (lexeme, _) = Assert.Single(result)
    Assert.Equal("abc", lexeme)

[<Fact>]
let Scan_Identifier_SourcePosition() =
    let reader = Reader()
    let result = reader.Scan("abc").ToArray()
    let struct (_, syntaxInfo) = Assert.Single(result)
    Assert.Equal(SourcePosition(0, 0, 0), syntaxInfo.Position)
    Assert.Equal(3, syntaxInfo.Span)

[<Fact>]
let Scan_List_SourcePosition() =
    let reader = Reader()
    match reader.Scan("(+ 2\n30)").ToArray() with
    | [| (_, si0); (_, si1); (_, si2); (_, si3); (_, si4) |] ->
        Assert.Equal(SourcePosition(0, 0, 0), si0.Position)
        Assert.Equal(1, si0.Span)
        Assert.Equal(SourcePosition(1, 0, 1), si1.Position)
        Assert.Equal(1, si1.Span)
        Assert.Equal(SourcePosition(3, 0, 3), si2.Position)
        Assert.Equal(1, si2.Span)
        Assert.Equal(SourcePosition(5, 1, 0), si3.Position)
        Assert.Equal(2, si3.Span)
        Assert.Equal(SourcePosition(7, 1, 2), si4.Position)
        Assert.Equal(1, si4.Span)
    | _ -> Assert.True(false, "Wrong number of elements in scan result")

[<Fact>]
let Scan_List() =
    let reader = Reader()
    match reader.Scan("(+ 2\n30)").ToArray() with
    | [| (l0, _); (l1, _); (l2, _); (l3, _); (l4, _) |] ->
        Assert.Equal("(", l0)
        Assert.Equal("+", l1)
        Assert.Equal("2", l2)
        Assert.Equal("30", l3)
        Assert.Equal(")", l4)
    | _ -> Assert.True(false, "Wrong number of elements in scan result")

[<Fact>]
let EvalToken_Number() =
    let reader = Reader()
    let struct (tokenType, value, _) = reader.EvalToken("12")
    Assert.Equal(TokenType.Number, tokenType)
    Assert.Equal<obj>(12L, value)

[<Fact>]
let EvalToken_NegativeNumber() =
    let reader = Reader()
    let struct (tokenType, value, _) = reader.EvalToken("-12")
    Assert.Equal(TokenType.Number, tokenType)
    Assert.Equal<obj>(-12L, value)

[<Fact>]
let EvalToken_Identifier() =
    let reader = Reader()
    let struct (tokenType, _, name) = reader.EvalToken("abc")
    Assert.Equal(TokenType.Identifier, tokenType)
    Assert.Equal("abc", name)

[<Fact>]
let EvalToken_Plus_Identifier() =
    let reader = Reader()
    let struct (tokenType, _, name) = reader.EvalToken("+")
    Assert.Equal(TokenType.Identifier, tokenType)
    Assert.Equal("+", name)

[<Fact>]
let EvalToken_LeftBracket() =
    let reader = Reader()
    let struct (tokenType, _, _) = reader.EvalToken("(")
    Assert.Equal(TokenType.LeftBracket, tokenType)

[<Fact>]
let EvalToken_RightBracket()=
    let reader = Reader()
    let struct (tokenType, _, _) = reader.EvalToken(")")
    Assert.Equal(TokenType.RightBracket, tokenType)

[<Fact>]
let ReadInteractive_Symbol_SourceInfo() =
    let reader = Reader()
    let result = reader.ReadInteractive([ "x" ])
    let object = Assert.Single(result.Objects)
    Assert.Equal(SourceType.String, object.SyntaxInfo.Source.Type)
    Assert.Null(object.SyntaxInfo.Source.FileName)

[<Fact>]
let ReadInteractive_Symbol_SyntaxInfo() =
    let reader = Reader()
    let result = reader.ReadInteractive([ "x" ])
    let object = Assert.Single(result.Objects)
    Assert.Equal(SourcePosition(0, 0, 0), object.SyntaxInfo.Position)
    Assert.Equal(1, object.SyntaxInfo.Span)

[<Fact>]
let ReadInteractive_Symbol() =
    let reader = Reader()
    let result = reader.ReadInteractive([ "x" ])
    Assert.True(result.Finished)
    let object = Assert.Single(result.Objects)
    let identifier = Assert.IsType<SyntaxIdentifier>(object)
    Assert.Equal("x", identifier.Name)

[<Fact>]
let ReadInteractive_TwoSymbols() =
    let reader = Reader()
    let result = reader.ReadInteractive([ "x y" ])
    Assert.True(result.Finished)
    Assert.Equal(2, result.Objects.Count)
    match result.Objects.ToArray() with
    | [| o0; o1 |] ->
        let identifier0 = Assert.IsType<SyntaxIdentifier>(o0)
        Assert.Equal("x", identifier0.Name)
        let identifier1 = Assert.IsType<SyntaxIdentifier>(o1)
        Assert.Equal("y", identifier1.Name)
    | _ -> ()

[<Fact>]
let ReadInteractive_TwoLines() =
    let reader = Reader()
    let result = reader.ReadInteractive([ "x"; "y" ])
    Assert.True(result.Finished)
    Assert.Equal(2, result.Objects.Count)
    match result.Objects.ToArray() with
    | [| o0; o1 |] ->
        let identifier0 = Assert.IsType<SyntaxIdentifier>(o0)
        Assert.Equal("x", identifier0.Name)
        let identifier1 = Assert.IsType<SyntaxIdentifier>(o1)
        Assert.Equal("y", identifier1.Name)
    | _ -> ()

[<Fact>]
let ReadInteractive_ListSplitIntoTwoLines() =
    let reader = Reader()
    let result = reader.ReadInteractive([ "("; ")" ])
    Assert.True(result.Finished)
    let object = Assert.Single(result.Objects)
    let list = Assert.IsType<SyntaxList>(object)
    Assert.Empty(list.Items)

[<Fact>]
let ReadInteractive_ListStart_NotFinished() =
    let reader = Reader()
    let result = reader.ReadInteractive([ "(" ])
    Assert.False(result.Finished)

