namespace BrightSword.CSharpExtensions.DiscriminatedUnion

module internal CodeGenerator =
    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax

    open BrightSword.RoslynWrapper.Common
    open BrightSword.RoslynWrapper.FieldDeclaration
    open BrightSword.RoslynWrapper.MethodDeclaration
    open BrightSword.RoslynWrapper.PropertyDeclaration
    open BrightSword.RoslynWrapper.ConstructorDeclaration
    open BrightSword.RoslynWrapper.ClassDeclaration
    open BrightSword.RoslynWrapper.ObjectCreation
    open BrightSword.RoslynWrapper.Conversion

    type SF = SyntaxFactory

    (*
    let private toFuncParameterName = toParameterName >> sprintf "%sFunc"

    let private toMatchFuncParameter = function
        | (e, []) -> (e |> toFuncParameterName, "Func<TResult>")
        | (c, args) -> (c|> toFuncParameterName, args |> Seq.toArray |> (fun (rg : System.String array) -> System.String.Join(",", rg)) |> sprintf "Func<%s, TResult>")

    let private toMatchFuncParameters du = du.DiscriminatedUnionMembers |> Seq.map (fun duMember -> toMatchFuncParameter duMember.unapply)


    let private toPrivateBaseValueProperty du =
        du.IsSubsetOf
        |> Option.map (fun b ->
            ``propg`` b "BaseValue" [``private``]
            :> MemberDeclarationSyntax)

    let private toMatchFunction typeParamName invocation du =
        let toMatchFuncParameter typeParamName (memberName, memberTypeList) =
            let matchFuncReturnType = memberTypeList |> List.fold (fun _ m -> sprintf "Func<%s, %s>" m typeParamName) (sprintf "Func<%s>" typeParamName) // BUG
            (memberName |> toFuncParameterName, matchFuncReturnType)
        let matchFuncParameters = du.DiscriminatedUnionMembers |> Seq.map (fun duMember -> toMatchFuncParameter typeParamName duMember.unapply)
        in
        ``method`` typeParamName "Match" ``<<`` [ typeParamName ] ``>>`` ``(`` matchFuncParameters ``)``
            [``public``; (invocation |> Option.fold (fun _ s -> ``override``) ``abstract``)]
            ``=>`` invocation
        :> MemberDeclarationSyntax
        |> Some

    let private toMatchFunctionDeclaration = toMatchFunction "TResult" None


    let private toChoiceClass du (memberName, argTypeList) =
        let choiceClassConstructor =
            match (du.IsSubsetOf, argTypeList) with
            | None, [] -> None
            | Some baseType, [] ->
                ``constructor`` memberName ``(`` [] ``)``
                    ``:`` [ (sprintf "%s.%s" baseType memberName) ]
                    [``public``]
                    ``{``
                        []
                    ``}``
                :> MemberDeclarationSyntax
                |> Some
            | None, args ->
                let ctorParameters = args |> Seq.map (fun (a : string) -> (a.ToLower(), a))
                let memberAssignments = args |> Seq.map (fun (a : string) -> (a <-- a.ToLower()) |> SF.ExpressionStatement :> StatementSyntax)
                in
                ``constructor`` memberName ``(`` ctorParameters ``)``
                    ``:`` []
                    [``public``]
                    ``{``
                        memberAssignments
                    ``}``
                :> MemberDeclarationSyntax
                |> Some
            | Some _, _ -> failwith "cannot extend a DU with constructor members"

        let choiceClassItemProperty =
            argTypeList
            |> Seq.map (fun argType ->
                ``propg`` argType argType [``private``]
                :> MemberDeclarationSyntax
                |> Some
                )
        let choiceClassMatchFunctionOverride =
            let invocation =
                let invocationParameterFuncArgumentList =
                    argTypeList
                    |> List.fold (fun _ s -> ([ s |> (SF.IdentifierName >> SF.Argument) ])) [] // BUG
                    |> (SF.SeparatedList >> SF.ArgumentList)
                in
                memberName
                |> toParameterName
                |> (toIdentifierName >> SF.InvocationExpression)
                |> (fun ie -> ie.WithArgumentList invocationParameterFuncArgumentList)
                |> SF.ArrowExpressionClause
            in
            du |> toMatchFunction "TResult" (Some invocation)

        let members = seq {
                yield choiceClassConstructor
                yield! choiceClassItemProperty
                yield choiceClassMatchFunctionOverride
            }
        in
        ``class`` memberName ``<<`` [] ``>>``
            ``:`` (Some (du |> toClassName)) ``,`` []
            [``public``]
            ``{``
                 (members |> Seq.choose (id))
            ``}``
            :> MemberDeclarationSyntax

    let toBaseCastOperator du =
        du.IsSubsetOf
        |> Option.map (fun baseType ->
                let ``value`` = "value" |> toIdentifierName
                let ``baseValue`` = "BaseValue" |> toIdentifierName
                let initializer = ``=>`` (``value`` <.> ``baseValue``)
                in
                ``explicit operator`` baseType ``(`` du.DiscriminatedUnionName ``)`` ``=>`` initializer
                :> MemberDeclarationSyntax
            )

    let private toChoiceTypeWrapperClass du =
        let innerClasses = du.DiscriminatedUnionMembers |> Seq.map (fun duMember -> toChoiceClass du duMember.unapply)
        in
        ``class``  "ChoiceTypes" ``<<`` [] ``>>``
            ``:`` None ``,`` []
            [``private``; ``static``]
            ``{``
                innerClasses
            ``}``
        :> MemberDeclarationSyntax
        |> Some

    let toAccessMember unionMember = function
        | (e, []) ->
            let initializer = ``:=`` (``new`` ["ChoiceTypes"; e] ``(`` [] ``)``)
            in
            ``field`` unionMember e
                [``public``; ``static``; ``readonly``]
                ``:=`` (Some initializer)
            :> MemberDeclarationSyntax
            |> Some
        | (c, args) ->
            let methodName = c |> sprintf "New%s"
            let ctorArguments = args |> Seq.map (fun (a : string) -> a.ToLower())
            let methodParameters = args |> Seq.map (fun (a : string) -> (a.ToLower(), a))
            let expression = ``=>`` (``new`` ["ChoiceTypes"; c]  ``(`` ctorArguments  ``)``)
            in
            ``method`` unionMember methodName ``<<`` [] ``>>`` ``(`` methodParameters ``)``
                [``public``; ``static``]
                ``=>`` (Some expression)
            :> MemberDeclarationSyntax
            |> Some

    let private toClassName du =
        if du.TypeParameters |> Seq.isEmpty then
            du.DiscriminatedUnionName
        else
            sprintf "%s<%s>" du.DiscriminatedUnionName (du.TypeParameters |> String.concat ",")
    *)
   
    let to_private_ctor du =
        let className = du.UnionTypeName.unapply
        seq {
            yield ``constructor`` className ``(`` [] ``)``
                    ``:`` []
                    [``private``]
                    ``{``
                        []
                    ``}``
                :> MemberDeclarationSyntax
        }       

    let build_class_declaration_syntax fns du = 
        let className = du.UnionTypeName.unapply   
        let genericTypeParams =
            du.UnionTypeParameters
            |> Seq.map (fun p -> p.unapply)
        let members = fns |> Seq.collect (fun f -> du |> f)
        in
        ``class`` className ``<<`` genericTypeParams ``>>``
            ``:`` None ``,`` []
            [``public``; ``abstract``; ``partial``]
            ``{``
                members
            ``}``    
    // <summary>
    // Entry-point to the C# DU generator.
    //
    // Generates the Roslyn <code>ClassDeclarationSyntax</code> for a given a UnionType AST. 
    //
    // Given 
    // <code lang="csharp">
    //     union Maybe{T} 
    //     {
    //         Some{T};
    //         None;
    //     }
    // </code>
    //
    // this function generates the syntax to define a class that looks like:
    //
    // <code lang="csharp">
    //    // class_declaration
    //    public abstract class Maybe{T}
    //    {
    //        // private_ctor
    //        private Maybe() {}
    //
    //        // match_function_abstract
    //        public abstract R Match{R}(Func{R} noneFunc, Func{T, R} someFunc);
    //
    //        // access_member
    //        public static readonly Maybe{T} None = new ChoiceTypes.None();
    //        public static Maybe{T} NewSome(T value) => new ChoiceTypes.Some(value);
    //
    //        // wrapper_type
    //        private static class ChoiceTypes
    //        {
    //            // choice_class (value)
    //            public class Some : Maybe{T}
    //            {
    //                // choice_class_ctor
    //                public Some(T value)
    //                {
    //                    Value = value;
    //                }
    //
    //                // value_property
    //                public T Value { get; }
    //
    //                // match_function_override
    //                public override R Match{R}(Func{R} noneFunc, Func{T, R} someFunc) => 
    //                    someFunc(Value);
    //
    //                // Value Semantics (https://msdn.microsoft.com/en-us/library/dd183755.aspx)
    //                // equals_object_override
    //                public override bool Equals(object other) =>
    //                    this.Equals(other as Some{T});
    //
    //                // equals_implementation
    //                public bool Equals(Some{T} other) =>
    //                    (Object.ReferenceEquals(this, other)
    //                    || (!Object.ReferenceEquals(this, null)
    //                        && (this.GetType() == other.GetType())
    //                        && (Value == other.Value)));
    //
    //                // hashcode_override
    //                public override int GetHashCode() =>
    //                    Value.GetHashCode();
    //
    //                // eq_operator
    //                public static bool operator ==(Some{T} left, Some{T} right) =>
    //                    (((left == null) && (right == null))
    //                    || ((left != null) && (left.Equals(right))));
    //
    //                // neq_operator
    //                public static bool operator !=(Some{T} left, Some{T} right) =>
    //                    (!(left == right));
    //            }
    //
    //            // choice_class (singleton)
    //            public class None : Maybe{T}
    //            {
    //                // choice_class_ctor
    //                public None {}
    //
    //                // match_function_override
    //                public override R Match{R}(Func{R} noneFunc, Func{T, R} someFunc) => 
    //                    noneFunc(Value);
    //
    //                // equals_object_override
    //                public override bool Equals(object other) =>
    //                    this.Equals(other as None);
    //
    //                // equals_implementation
    //                public bool Equals(None other) => true;
    //
    //                // hashcode_override
    //                public override int GetHashCode() =>
    //                    this.GetType().FullName.GetHashCode();
    //
    //                // eq_operator
    //                public static bool operator ==(None left, None right) => true;
    //
    //                // neq_operator
    //                public static bool operator !=(None left, None right) => false;
    //            }
    //        }
    //    }
    // </code>
    // 
    // </summary>
    // <param name="du">The UnionType AST to generate code for</param>
    let to_class_declaration du =
        let fns =
            [
                to_private_ctor
            ]
        in
        build_class_declaration_syntax fns du
        
        (*
        let to_match_function invocation du =
            let toMatchFuncParameter typeParamName (memberName, memberTypeList) =
                let matchFuncReturnType = memberTypeList |> List.fold (fun _ m -> sprintf "Func<%s, %s>" m typeParamName) (sprintf "Func<%s>" typeParamName) // BUG
                (memberName |> toFuncParameterName, matchFuncReturnType)
            let matchFuncParameters = du.DiscriminatedUnionMembers |> Seq.map (fun duMember -> toMatchFuncParameter "TResult" duMember.unapply)
            in
            ``method`` "TResult" "Match" ``<<`` [ "TResult" ] ``>>`` ``(`` matchFuncParameters ``)``
                [``public``; (invocation |> Option.fold (fun _ s -> ``override``) ``abstract``)]
                ``=>`` invocation
            :> MemberDeclarationSyntax
            |> Some

        let to_match_function_abstract = 
            to_match_function None

        let members = seq {
                yield du |> to_private_ctor
                yield du |> to_match_function_abstract

                yield! du.UnionMembers |> Seq.map (fun duMember -> toAccessMember (du |> toClassName) duMember.unapply)
                yield du |> toPrivateBaseValueProperty
                yield du |> toMatchFunctionDeclaration
                yield du |> toBaseCastOperator
                yield du |> toChoiceTypeWrapperClass
            }

        let genericTypeParams =
            du.UnionTypeParameters
            |> Seq.map (fun p -> p.unapply)
        in
        ``class`` className ``<<`` genericTypeParams ``>>``
            ``:`` None ``,`` []
            [``public``; ``abstract``; ``partial``]
            ``{``
                (members |> Seq.choose id)
            ``}``
        *)