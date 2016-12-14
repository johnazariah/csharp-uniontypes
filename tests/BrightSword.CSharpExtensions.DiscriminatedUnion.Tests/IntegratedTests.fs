namespace BrightSword.CSharpExtensions.DiscriminatedUnion.Tests

open BrightSword.CSharpExtensions.DiscriminatedUnion.AST
open BrightSword.CSharpExtensions.DiscriminatedUnion.Parser
open BrightSword.CSharpExtensions.DiscriminatedUnion.CodeGenerator
open BrightSword.RoslynWrapper
open NUnit.Framework

module IntegratedTests = 

    let namespace_to_code namespace_declaration_syntax = 
        ``compilation unit`` 
            [ 
                namespace_declaration_syntax
            ] 
        |> generateCodeToString

    let maybe_of_T = @"
namespace CoolMonads 
{
    using System;

    union Maybe<T> 
    {
        case class Some<T>;
        case object None;
    }
}"

    [<Test>]
    let ``parse-and-code-gen: maybe``() = 
        maybe_of_T 
        |> parseTextToNamespace
        |> Option.map (to_namespace_declaration >> namespace_to_code >> printf "%s")
        |> ignore