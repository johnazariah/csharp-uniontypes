namespace CSharp.UnionTypes

open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Syntax
open BrightSword.RoslynWrapper

[<AutoOpen>]
module UnionTypeClassDeclarationBuilder =
    let union_typename (du : UnionType) =
        let class_name = du.UnionTypeName.unapply
        let type_parameters = du.UnionTypeParameters |> Seq.map (fun p -> p.unapply)
        if Seq.isEmpty type_parameters then
            (``type`` class_name)
        else
            ``generic type`` class_name ``<<`` type_parameters ``>>`` :> NameSyntax

    let to_base_value (du : UnionType) =
        du.BaseType
        |> Option.map (fun b -> ``field`` b.CSharpTypeName "_base" [``private``; ``readonly``] None :> MemberDeclarationSyntax)
        |> Option.fold (fun _ x -> [x]) []

    let to_private_ctor (du : UnionType) =
        let ctor_args =
            let ctor_args_for_constrained_type _ (base_type : FullTypeName) =
                [ ("value", ``type`` [base_type.CSharpTypeName]) ]
            in
            du.BaseType
            |> Option.fold ctor_args_for_constrained_type []

        let ctor_assigns =
            let ctor_assigns_for_constrained_type _ _ =
                [ (ident "_base" <-- ident "value") |> SyntaxFactory.ExpressionStatement :> StatementSyntax ]
            in
            du.BaseType
            |> Option.fold ctor_assigns_for_constrained_type  []

        let className = du.UnionTypeName.unapply
        in
        [
            ``constructor`` className ``(`` ctor_args ``)`` ``:`` []
                [ ``private`` ]
                ``{``
                    ctor_assigns
                ``}``
                :> MemberDeclarationSyntax
        ]

    let to_access_members (du : UnionType) =
        let union_name = du.UnionClassNameWithTypeArgs

        let to_access_member (um : UnionMember) =
            let member_name = um.UnionMemberAccessName
            let class_name = um.ChoiceClassName

            let to_access_member_method _ (t : FullTypeName) =
                let method_arg_name = "value"
                let method_arg_type = t.CSharpTypeName
                let initialization_expression =
                    ``=>``(``new`` (``type`` [ "ChoiceTypes"; class_name ]) ``(`` [ ident method_arg_name ] ``)``)

                ``arrow_method`` union_name member_name ``<<`` [] ``>>`` ``(`` [ (method_arg_name, ``type`` method_arg_type) ] ``)``
                    [ ``public``; ``static`` ]
                    (Some initialization_expression)
                    :> MemberDeclarationSyntax

            let to_access_member_field =
                let field_initializer = ``:=`` (``new`` (``type`` [ "ChoiceTypes"; class_name ]) ``(`` [] ``)``)
                field union_name member_name
                    [ ``public``; ``static``; readonly ]
                    (Some field_initializer)
                    :> MemberDeclarationSyntax

            um.MemberArgumentType |> Option.fold to_access_member_method to_access_member_field
        du.UnionMembers |> List.map to_access_member

    let to_match_function_abstract = to_match_function None

    let to_wrapper_type (du : UnionType) =
        let choice_classes = du.UnionMembers |> Seq.map (to_choice_class du)
        [
            ``class`` "ChoiceTypes" ``<<`` [] ``>>``
                ``:`` None ``,`` []
                [ ``private``; ``static``; ``partial`` ]
                ``{``
                    choice_classes
                ``}``
                :> MemberDeclarationSyntax
        ]

    //  public bool Equals(Maybe<T> other) => Equals(other as object);
    let to_equatable_equals_method (du : UnionType) =
        [
            ``arrow_method`` "bool" "Equals" ``<<`` [] ``>>`` ``(`` [ ("other", union_typename du) ]``)``
                [``public``]
                (Some (``=>`` (``invoke`` (ident "Equals") ``(`` [ (ident "other") |~> "object" ] ``)``)))
                :> MemberDeclarationSyntax
        ]

    //  public bool Equals(object other, IEqualityComparer comparer) => Equals(other);
    let to_structural_equality_equals_method _ =
        [
            ``arrow_method`` "bool" "Equals" ``<<`` [] ``>>`` ``(`` [ ("other", ``type`` "object"); ("comparer", ``type`` "IEqualityComparer") ]``)``
                [``public``]
                (Some (``=>`` (``invoke`` (ident "Equals") ``(`` [ (ident "other") ] ``)``)))
                :> MemberDeclarationSyntax
        ]

    //  public int GetHashCode(IEqualityComparer comparer) => GetHashCode();
    let to_structural_equality_gethashcode_method _ =
        [
            ``arrow_method`` "int" "GetHashCode" ``<<`` [] ``>>`` ``(`` [ ("comparer", ``type`` "IEqualityComparer") ]``)``
                [``public``]
                (Some (``=>`` (``invoke`` (ident "GetHashCode") ``(`` [] ``)``)))
                :> MemberDeclarationSyntax
        ]

    //   public static bool operator ==(Maybe<T> left, Maybe<T> right) => left?.Equals(right) ?? false;
    let to_eq_operator du =
        [
            ``operator ==`` ("left", "right", union_typename du)
                (``=>`` (((ident "left") <?.> ("Equals", [ ident "right" ])) <??> ``false``))
                :> MemberDeclarationSyntax
        ]

    //   public static bool operator !=(Maybe<T> left, Maybe<T> right) => !(left == right);
    let to_neq_operator du =
        [
            ``operator !=`` ("left", "right", union_typename du)
                (``=>`` (! ((ident "left") <==> (ident "right"))))

                :> MemberDeclarationSyntax
        ]

    // public static explicit operator Maybe<T>(SingleValue<T> value) => value._base;
    let to_base_cast du =
        du.BaseType
        |> Option.map (fun b ->
            ``explicit operator`` b.CSharpTypeName ``(`` (``type`` du.UnionClassNameWithTypeArgs) ``)``
                (``=>`` (ident "value" <|.|> "_base"))
                :> MemberDeclarationSyntax)
        |> Option.fold (fun _ m -> [m]) []

    let to_class_declaration_internal fns du =
        let class_name = du.UnionTypeName.unapply
        let type_parameters = du.UnionTypeParameters |> List.map (fun p -> p.unapply)
        let full_class_name_string = sprintf "%s%s" class_name (if type_parameters = [] then "" else sprintf "<%s>" (String.concat ", " type_parameters))
        let members = fns |> Seq.collect (fun f -> du |> (f >> List.toSeq))
        ``class`` class_name ``<<`` type_parameters ``>>``
            ``:`` None ``,`` [ sprintf "IEquatable<%s>" full_class_name_string; "IStructuralEquatable" ]
            [ ``public``; ``abstract``; ``partial`` ]
            ``{``
                members
            ``}``
            :> MemberDeclarationSyntax

    let to_class_declaration du =
        let fns =
            [
                to_base_value
                to_private_ctor
                to_match_function_abstract
                to_access_members
                to_wrapper_type
                to_equatable_equals_method
                to_structural_equality_equals_method
                to_structural_equality_gethashcode_method
                to_eq_operator
                to_neq_operator
                to_base_cast
            ]
        to_class_declaration_internal fns du
