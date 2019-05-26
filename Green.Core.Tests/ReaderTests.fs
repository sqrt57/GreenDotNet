module ReaderTests

open System.Linq
open Xunit
open Green
open Green.Read

[<Fact>]
let Read_Empty() =
    let result = read ""
    Assert.Empty(result)

[<Fact>]
let Read_Atom_SourceInfo() =
    let result = read "58" |> Seq.toList
    let onlyResult = Assert.Single(result)
    Assert.Equal(SourceType.String, onlyResult.SyntaxInfo.Source.Type)
    Assert.Null(onlyResult.SyntaxInfo.Source.FileName)

[<Fact>]
let Read_Atom_SyntaxInfo() =
    let result = read "58" |> Seq.toList
    let onlyResult = Assert.Single(result)
    Assert.Equal(SourcePosition(0, 0, 0), onlyResult.SyntaxInfo.Position)
    Assert.Equal(2, onlyResult.SyntaxInfo.Span)

[<Fact>]
let Read_List_SyntaxInfo() =
    let result = read "(+ 2 3)" |> Seq.toList
    let onlyResult = Assert.Single(result)
    Assert.Equal(SourcePosition(0, 0, 0), onlyResult.SyntaxInfo.Position)
    Assert.Equal(7, onlyResult.SyntaxInfo.Span)

[<Fact>]
let Read_Number() =
    let result = read "5" |> Seq.toList
    let onlyResult = Assert.Single(result)
    let constant = Assert.IsType<SyntaxConstant>(onlyResult)
    Assert.Equal<obj>(5L, constant.Value)

[<Fact>]
let Read_Symbol() =
    let result = read "x" |> Seq.toList
    let onlyResult = Assert.Single(result)
    let identifier = Assert.IsType<SyntaxIdentifier>(onlyResult)
    Assert.Equal("x", identifier.Name)

[<Fact>]
let Read_EmptyList() =
    let result = read "()" |> Seq.toList
    let onlyResult = Assert.Single(result)
    let list = Assert.IsType<SyntaxList>(onlyResult)
    Assert.Empty(list.Items)

[<Fact>]
let Read_List() =
    let result = read "(+ 2 3)" |> Seq.toList
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
    let result = (scan "").ToArray()
    Assert.Empty(result)

[<Fact>]
let Scan_Whitespace() =
    let result = (scan " ").ToArray()
    Assert.Empty(result)

[<Fact>]
let Scan_Number_SourceInfo() =
    let result = (scan "15").ToArray()
    let struct (_, syntaxInfo) = Assert.Single(result)
    Assert.Equal(SourceType.String, syntaxInfo.Source.Type)
    Assert.Null(syntaxInfo.Source.FileName)

[<Fact>]
let Scan_Number_SourcePosition() =
    let result = (scan "15").ToArray()
    let struct (_, syntaxInfo) = Assert.Single(result)
    Assert.Equal(SourcePosition(0, 0, 0), syntaxInfo.Position)
    Assert.Equal(2, syntaxInfo.Span)

[<Fact>]
let Scan_Number() =
    let result = (scan "15").ToArray()
    let struct (lexeme, _) = Assert.Single(result)
    Assert.Equal("15", lexeme)

[<Fact>]
let Scan_Identifier() =
    let result = (scan "abc").ToArray()
    let struct (lexeme, _) = Assert.Single(result)
    Assert.Equal("abc", lexeme)

[<Fact>]
let Scan_Identifier_SourcePosition() =
    let result = (scan "abc").ToArray()
    let struct (_, syntaxInfo) = Assert.Single(result)
    Assert.Equal(SourcePosition(0, 0, 0), syntaxInfo.Position)
    Assert.Equal(3, syntaxInfo.Span)

[<Fact>]
let Scan_List_SourcePosition() =
    match (scan "(+ 2\n30)").ToArray() with
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
    let result = (scan "(+ 2\n30)").ToArray()
    Assert.Equal(5, Array.length result)
    match result with
    | [| (l0, _); (l1, _); (l2, _); (l3, _); (l4, _) |] ->
        Assert.Equal("(", l0)
        Assert.Equal("+", l1)
        Assert.Equal("2", l2)
        Assert.Equal("30", l3)
        Assert.Equal(")", l4)
    | _ -> Assert.True(false, "Wrong number of elements in scan result")

[<Fact>]
let EvalToken_Number() =
    let struct (tokenType, value, _) = evalToken "12"
    Assert.Equal(TokenType.Number, tokenType)
    Assert.Equal<obj>(12L, value)

[<Fact>]
let EvalToken_NegativeNumber() =
    let struct (tokenType, value, _) = evalToken "-12"
    Assert.Equal(TokenType.Number, tokenType)
    Assert.Equal<obj>(-12L, value)

[<Fact>]
let EvalToken_Identifier() =
    let struct (tokenType, _, name) = evalToken "abc"
    Assert.Equal(TokenType.Identifier, tokenType)
    Assert.Equal("abc", name)

[<Fact>]
let EvalToken_Plus_Identifier() =
    let struct (tokenType, _, name) = evalToken "+"
    Assert.Equal(TokenType.Identifier, tokenType)
    Assert.Equal("+", name)

[<Fact>]
let EvalToken_LeftBracket() =
    let struct (tokenType, _, _) = evalToken "("
    Assert.Equal(TokenType.LeftBracket, tokenType)

[<Fact>]
let EvalToken_RightBracket()=
    let struct (tokenType, _, _) = evalToken ")"
    Assert.Equal(TokenType.RightBracket, tokenType)

[<Fact>]
let ReadInteractive_Symbol_SourceInfo() =
    match readInteractive [ "x" ] with
    | None -> Assert.True(false, "Should return Some(objects)")
    | Some objects ->
        let object = Assert.Single(objects)
        Assert.Equal(SourceType.String, object.SyntaxInfo.Source.Type)
        Assert.Null(object.SyntaxInfo.Source.FileName)

[<Fact>]
let ReadInteractive_Symbol_SyntaxInfo() =
    match readInteractive [ "x" ] with
    | None -> Assert.True(false, "Should return Some(objects)")
    | Some objects ->
        let object = Assert.Single(objects)
        Assert.Equal(SourcePosition(0, 0, 0), object.SyntaxInfo.Position)
        Assert.Equal(1, object.SyntaxInfo.Span)

[<Fact>]
let ReadInteractive_Symbol() =
    match readInteractive [ "x" ] with
    | None -> Assert.True(false, "Should return Some(objects)")
    | Some objects ->
        let object = Assert.Single(objects)
        let identifier = Assert.IsType<SyntaxIdentifier>(object)
        Assert.Equal("x", identifier.Name)

[<Fact>]
let ReadInteractive_TwoSymbols() =
    match readInteractive [ "x y" ] with
    | None -> Assert.True(false, "Should return Some(objects)")
    | Some objects ->
        Assert.Equal(2, Seq.length objects)
        match objects.ToArray() with
        | [| o0; o1 |] ->
            let identifier0 = Assert.IsType<SyntaxIdentifier>(o0)
            Assert.Equal("x", identifier0.Name)
            let identifier1 = Assert.IsType<SyntaxIdentifier>(o1)
            Assert.Equal("y", identifier1.Name)
        | _ -> ()

[<Fact>]
let ReadInteractive_TwoLines() =
    match readInteractive [ "x"; "y" ] with
    | None -> Assert.True(false, "Should return Some(objects)")
    | Some objects ->
        Assert.Equal(2, Seq.length objects)
        match objects.ToArray() with
        | [| o0; o1 |] ->
            let identifier0 = Assert.IsType<SyntaxIdentifier>(o0)
            Assert.Equal("x", identifier0.Name)
            let identifier1 = Assert.IsType<SyntaxIdentifier>(o1)
            Assert.Equal("y", identifier1.Name)
        | _ -> ()

[<Fact>]
let ReadInteractive_ListSplitIntoTwoLines() =
    match readInteractive [ "("; ")" ] with
    | None -> Assert.True(false, "Should return Some(objects)")
    | Some objects ->
        let object = Assert.Single(objects)
        let list = Assert.IsType<SyntaxList>(object)
        Assert.Empty(list.Items)

[<Fact>]
let ReadInteractive_ListStart_NotFinished() =
    let result = readInteractive [ "(" ]
    Assert.Equal(result, None)
    match result with
    | None -> ()
    | Some _ -> Assert.True(false, "Should return None")
