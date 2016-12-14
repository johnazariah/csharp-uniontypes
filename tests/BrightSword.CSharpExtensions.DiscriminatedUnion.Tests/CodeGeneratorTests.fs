namespace BrightSword.CSharpExtensions.DiscriminatedUnion.Tests

open BrightSword.CSharpExtensions.DiscriminatedUnion.AST
open BrightSword.CSharpExtensions.DiscriminatedUnion.CodeGenerator
open BrightSword.RoslynWrapper
open NUnit.Framework
open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Syntax

type SF = SyntaxFactory

module CodeGeneratorTests = 
    let maybe_of_T = 
        { UnionTypeName = UnionTypeName "Maybe"
          UnionTypeParameters = [ TypeArgument "T" ]
          UnionMembers = 
              [ { MemberName = UnionMemberName "None"
                  MemberArgumentType = None }
                { MemberName = UnionMemberName "Some"
                  MemberArgumentType = 
                      Some { FullyQualifiedTypeName = "T"
                             TypeArguments = [] } } ]
          BaseType = None }
    
    let class_to_code class_declaration_syntax = 
        ``compilation unit`` 
            [ ``namespace`` "DU.Tests" ``{`` [] [ class_declaration_syntax ] ``}`` :> MemberDeclarationSyntax ] 
        |> generateCodeToString
    
    [<Test>]
    let ``code-gen: private constructor``() = 
        maybe_of_T
        |> build_class_declaration_syntax [ to_private_ctor ]
        |> class_to_code
        |> printf "%s"
    
    [<Test>]
    let ``code-gen: match function abstract``() = 
        maybe_of_T
        |> build_class_declaration_syntax [ to_match_function_abstract ]
        |> class_to_code
        |> printf "%s"
    
    [<Test>]
    let ``code-gen: access members``() = 
        maybe_of_T
        |> build_class_declaration_syntax [ to_access_members ]
        |> class_to_code
        |> printf "%s"
    
    [<Test>]
    let ``code-gen: wrapper type``() = 
        maybe_of_T
        |> build_class_declaration_syntax [ to_wrapper_type ]
        |> class_to_code
        |> printf "%s"
    
    [<Test>]
    let ``code-gen: complete``() = 
        maybe_of_T
        |> to_class_declaration
        |> class_to_code
        |> printf "%s"
