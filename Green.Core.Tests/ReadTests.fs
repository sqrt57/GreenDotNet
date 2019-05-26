module ReaderTests

open System.Linq
open Xunit
open Green.Source
open Green.Source.Lex
open Green.Source.Parse
open Green.Read

[<Fact>]
let Read_Empty() =
    let result = read ""
    Assert.Empty(result)

[<Fact>]
let Read_Atom_SyntaxInfo() =
    let result = read "58" |> Seq.toList
    let {info=syntaxInfo} = Assert.Single(result)
    Assert.Equal({left={pos=0; line=0; col=0}; right={pos=2; line=0; col=2}}, syntaxInfo)

[<Fact>]
let Read_List_SyntaxInfo() =
    let result = read "(+ 2 3)" |> Seq.toList
    let {info=syntaxInfo} = Assert.Single(result)
    Assert.Equal({left={pos=0; line=0; col=0}; right={pos=7; line=0; col=7}}, syntaxInfo)

[<Fact>]
let Read_Number() =
    let result = read "5" |> Seq.toList
    let {syntax=syntax} = Assert.Single(result)
    Assert.Equal(syntax, Syntax.Constant 5L)

[<Fact>]
let Read_Symbol() =
    let result = read "x" |> Seq.toList
    let {syntax=syntax} = Assert.Single(result)
    Assert.Equal(Syntax.Identifier "x", syntax)

[<Fact>]
let Read_EmptyList() =
    let result = read "()" |> Seq.toList
    let {syntax=syntax} = Assert.Single(result)
    Assert.Equal(Syntax.List [], syntax)

[<Fact>]
let Read_List() =
    let result = read "(+ 2 3)" |> Seq.toList
    let {syntax=syntax} = Assert.Single(result)
    match syntax with
    | Syntax.List list ->
        Assert.Equal(3, List.length list)
        match list with
        | [{syntax=s0}; {syntax=s1}; {syntax=s2}] ->
            Assert.Equal(Syntax.Identifier "+", s0)
            Assert.Equal(Syntax.Constant 2L, s1)
            Assert.Equal(Syntax.Constant 3L, s2)
        | _ -> ()
    | _ -> Assert.True(false, "Should return List")

[<Fact>]
let Scan_Empty() =
    let result = scan ""
    Assert.Empty(result)

[<Fact>]
let Scan_Whitespace() =
    let result = scan " "
    Assert.Empty(result)

[<Fact>]
let Scan_Number_SourcePosition() =
    let result = (scan "15").ToArray()
    let struct (_, syntaxInfo) = Assert.Single(result)
    Assert.Equal({left={pos=0; line=0; col=0}; right={pos=2; line=0; col=2}}, syntaxInfo)

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
    Assert.Equal({left={pos=0; line=0; col=0}; right={pos=3; line=0; col=3}}, syntaxInfo)

[<Fact>]
let Scan_List_SourcePosition() =
    match scan "(+ 2\n30)" |> Seq.toList with
    | [(_,si0); (_,si1); (_,si2); (_,si3); (_,si4)] ->
        Assert.Equal({left={pos=0; line=0; col=0}; right={pos=1; line=0; col=1}}, si0)
        Assert.Equal({left={pos=1; line=0; col=1}; right={pos=2; line=0; col=2}}, si1)
        Assert.Equal({left={pos=3; line=0; col=3}; right={pos=4; line=0; col=4}}, si2)
        Assert.Equal({left={pos=5; line=1; col=0}; right={pos=7; line=1; col=2}}, si3)
        Assert.Equal({left={pos=7; line=1; col=2}; right={pos=8; line=1; col=3}}, si4)
    | _ -> Assert.True(false, "Wrong number of elements in scan result")

[<Fact>]
let Scan_List() =
    let result = scan "(+ 2\n30)" |> Seq.toList
    Assert.Equal(5, List.length result)
    match result with
    | [ (l0, _); (l1, _); (l2, _); (l3, _); (l4, _) ] ->
        Assert.Equal("(", l0)
        Assert.Equal("+", l1)
        Assert.Equal("2", l2)
        Assert.Equal("30", l3)
        Assert.Equal(")", l4)
    | _ -> Assert.True(false, "Wrong number of elements in scan result")

[<Fact>]
let EvalToken_Number() =
    match evalToken "12" with
    | Number value -> Assert.Equal<obj>(12L, value)
    | _ -> Assert.True(false, "Should return Number")

[<Fact>]
let EvalToken_NegativeNumber() =
    match evalToken "-12" with
    | Number value -> Assert.Equal<obj>(-12L, value)
    | _ -> Assert.True(false, "Should return Number")

[<Fact>]
let EvalToken_Identifier() =
    match evalToken "abc" with
    | Token.Identifier value -> Assert.Equal<obj>("abc", value)
    | _ -> Assert.True(false, "Should return Identifier")

[<Fact>]
let EvalToken_Plus_Identifier() =
    match evalToken "+" with
    | Token.Identifier value -> Assert.Equal<obj>("+", value)
    | _ -> Assert.True(false, "Should return Identifier")

[<Fact>]
let EvalToken_LeftBracket() =
    let token = evalToken "("
    Assert.Equal(LeftBracket, token)

[<Fact>]
let EvalToken_RightBracket()=
    let token = evalToken ")"
    Assert.Equal(RightBracket, token)

[<Fact>]
let ReadInteractive_Symbol_SyntaxInfo() =
    match readInteractive ["x"] with
    | None -> Assert.True(false, "Should return Some objects")
    | Some objects ->
        let {info=syntaxInfo} = Assert.Single(objects)
        Assert.Equal({left={pos=0; line=0; col=0}; right={pos=1; line=0; col=1}}, syntaxInfo)

[<Fact>]
let ReadInteractive_Symbol() =
    match readInteractive ["x"] with
    | None -> Assert.True(false, "Should return Some objects")
    | Some objects ->
        let {syntax=syntax} = Assert.Single(objects)
        Assert.Equal(Syntax.Identifier "x", syntax)

[<Fact>]
let ReadInteractive_TwoSymbols() =
    match readInteractive ["x y"] with
    | None -> Assert.True(false, "Should return Some(objects)")
    | Some objects ->
        Assert.Equal(2, Seq.length objects)
        match objects with
        | [{syntax=s0}; {syntax=s1}] ->
            Assert.Equal(Syntax.Identifier "x", s0)
            Assert.Equal(Syntax.Identifier "y", s1)
        | _ -> ()

[<Fact>]
let ReadInteractive_TwoLines() =
    match readInteractive ["x"; "y"] with
    | None -> Assert.True(false, "Should return Some(objects)")
    | Some objects ->
        Assert.Equal(2, Seq.length objects)
        match objects with
        | [{syntax=s0}; {syntax=s1}] ->
            Assert.Equal(Syntax.Identifier "x", s0)
            Assert.Equal(Syntax.Identifier "y", s1)
        | _ -> ()

[<Fact>]
let ReadInteractive_ListSplitIntoTwoLines() =
    match readInteractive ["("; ")"] with
    | None -> Assert.True(false, "Should return Some(objects)")
    | Some objects ->
        let {syntax=syntax} = Assert.Single(objects)
        Assert.Equal(Syntax.List [], syntax)

[<Fact>]
let ReadInteractive_ListStart_NotFinished() =
    let result = readInteractive ["("]
    Assert.Equal(None, result)