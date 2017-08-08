namespace CSharp.AlgebraicTypes

open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Syntax
open BrightSword.RoslynWrapper

[<AutoOpen>]
module NamespaceDeclarationBuilder =
    let to_namespace_declaration ns =
        let pragma_trivia =
            SyntaxFactory.PragmaWarningDirectiveTrivia(SyntaxFactory.Token(SyntaxKind.DisableKeyword), true)

        let disable_warnings =
            [
                "CS0660"
                "CS0661"
            ]
            |> List.map (SyntaxFactory.IdentifierName >> (fun i -> i :> ExpressionSyntax) >> SyntaxFactory.SingletonSeparatedList >> pragma_trivia.WithErrorCodes >> SyntaxFactory.Trivia)

        let nsd =
            ``namespace`` ns.NamespaceName.unapply
                ``{``
                    ((ns.Usings |> List.map (fun u -> u.unapply)) @ ["System"; "System.Collections"] |> Set.ofList |> Set.toList)
                    (ns.Unions |> List.map to_union_class_declaration)
                ``}``
        in
        nsd.WithNamespaceKeyword(
            SyntaxFactory.Token(
                SyntaxFactory.TriviaList(disable_warnings),
                SyntaxKind.NamespaceKeyword,
                SyntaxFactory.TriviaList()))
