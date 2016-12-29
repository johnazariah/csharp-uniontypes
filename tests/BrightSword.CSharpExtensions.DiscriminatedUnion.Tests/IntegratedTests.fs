namespace BrightSword.CSharpExtensions.DiscriminatedUnion.Tests

open System.Text.RegularExpressions

open BrightSword.RoslynWrapper

open BrightSword.CSharpExtensions.DiscriminatedUnion.AST
open BrightSword.CSharpExtensions.DiscriminatedUnion.Parser
open BrightSword.CSharpExtensions.DiscriminatedUnion.CodeGenerator

open NUnit.Framework


module IntegratedTests = 

    let namespace_to_code namespace_declaration_syntax = 
        ``compilation unit`` 
            [ 
                namespace_declaration_syntax
            ] 
        |> generateCodeToString

    let maybe_of_T = @"
namespace DU.Tests 
{
    using System;

    union Maybe<T> { None | Some<T> }
}"

    [<Test>]
    let ``parse-and-code-gen: maybe``() =         
        let actual = 
            maybe_of_T 
            |> (parseTextToNamespace >> to_namespace_declaration >> namespace_to_code)
        in
        Assert.AreEqual(Regex.Replace(CodeGeneratorTests.COMPLETE_EXPECTED, "(?<!\r)\n", "\r\n"), Regex.Replace(actual, "(?<!\r)\n", "\r\n"))
