namespace BrightSword.CSharpExtensions.DiscriminatedUnion.Tests

open BrightSword.CSharpExtensions.DiscriminatedUnion.AST
open BrightSword.CSharpExtensions.DiscriminatedUnion.CodeGenerator
open BrightSword.RoslynWrapper
open NUnit.Framework

module IntegratedTests = 
    let maybe_of_T = @"
namespace Foo 
{
    using System;

    union Maybe<T> 
    {
        case class Some<T>;
        case object None;
    }
}"

    maybe_of_T 
    |> parseTextToNamespace
    |> 