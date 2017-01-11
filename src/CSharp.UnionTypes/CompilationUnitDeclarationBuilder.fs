namespace CSharp.UnionTypes

open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Syntax
open BrightSword.RoslynWrapper

[<AutoOpen>]
module CompilationUnitDeclarationBuilder =
    let namespace_to_code namespace_declaration_syntax =
        ``compilation unit``
            [
                namespace_declaration_syntax
            ]
        |> generateCodeToString

