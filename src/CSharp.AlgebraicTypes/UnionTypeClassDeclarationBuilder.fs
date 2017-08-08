namespace CSharp.AlgebraicTypes

open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Syntax
open BrightSword.RoslynWrapper

[<AutoOpen>]
module UnionMemberClassDeclarationBuilder =
    let pick_value_or_singleton fv fs um =
        um.MemberArgumentType |> Option.fold fv fs

    let ctor du (um : UnionMember) =
        let member_name = um.ChoiceClassName
        let (args, assignments) =
            match um.MemberArgumentType with
            | Some t ->
                let arg_type_name = t.CSharpTypeName
                ([("value", ``type`` arg_type_name)], [statement (ident "Value" <-- ident "value")])
            | None -> ([], [])

        let baseargs =
            du.BaseType
            |> Option.map (fun b ->
                um.MemberArgumentType
                |> Option.fold (fun seed _ -> sprintf "%s(value)" seed) (sprintf "%s.%s" b.CSharpTypeName um.ValueConstructor))
            |> Option.fold (fun _ b -> [b]) []

        match (args, assignments, baseargs) with
        | ([], [], []) -> []
        | _ ->
            [
                ``constructor`` member_name ``(`` args ``)``
                    ``:`` baseargs
                    [ ``public`` ]
                    ``{``
                        assignments
                    ``}``
                    :> MemberDeclarationSyntax
            ]

    let value_property _ um =
        let value_property_value _ (t: FullTypeName) =
            let arg_type_name = t.CSharpTypeName
            [
                propg arg_type_name "Value"
                    [ ``private`` ]
                    :> MemberDeclarationSyntax
            ]

        let value_property_singleton = []

        um
        |> pick_value_or_singleton value_property_value value_property_singleton

    let match_function_override du um =
        let argument_names = um.MemberArgumentType |> Option.fold (fun _ _ -> [ ident "Value" ]) []
        let match_function_name = um.MemberName.unapply |> to_match_function_parameter_name
        let invocation = ``=>`` (``invoke`` (ident match_function_name) ``(`` argument_names ``)`` )

        du
        |> to_match_function (Some invocation)

    let equals_override _ (um : UnionMember) =
        let member_name = um.ChoiceClassName

        let equality_expression_builder base_expression _ =
            let other_value_is_same = ``invoke`` (ident "Value.Equals") ``(`` [ ``((`` (``cast`` member_name (ident "other")) ``))`` <|.|> "Value" ] ``)``
            base_expression <&&> other_value_is_same

        let equality_expression =
            um.MemberArgumentType
            |> Option.fold equality_expression_builder (``is`` member_name (ident "other"))

        [
            ``arrow_method`` "bool" "Equals" ``<<`` [] ``>>`` ``(`` [ ("other", ``type`` "object") ]``)``
                [``public``; ``override``]
                (Some (``=>`` equality_expression))
                :> MemberDeclarationSyntax
        ]

    let hashcode_override _ um =
        let hashcode_expression_builder base_expression _ =
            base_expression <^> ``((`` ((ident "Value" <?.> ("GetHashCode", [])) <??> (literal "null" <.> ("GetHashCode", []))) ``))``

        let get_hash_code_expression =
            ``invoke`` (ident "GetType().FullName.GetHashCode") ``(`` [] ``)``

        let hashcode_expression =
            um.MemberArgumentType
            |> Option.fold hashcode_expression_builder get_hash_code_expression

        [
            ``arrow_method`` "int" "GetHashCode" ``<<`` [] ``>>`` ``(`` []``)``
                [``public``; ``override``]
                (Some (``=>`` hashcode_expression))
                :> MemberDeclarationSyntax
        ]

    let tostring_override _ um =
        let member_name = um.MemberName.unapply
        let string_expression_value _ _ =
            ``invoke`` (ident "String.Format") ``(`` [ (literal (sprintf "%s {0}" member_name)) :> ExpressionSyntax; ident "Value" :> ExpressionSyntax ] ``)``
        let string_expression_singleton =
            literal (sprintf ("%s") member_name)
            :> ExpressionSyntax
        let string_expression =
            um
            |> pick_value_or_singleton string_expression_value string_expression_singleton
        [
            ``arrow_method`` "string" "ToString" ``<<`` [ ] ``>>`` ``(`` [] ``)``
                [ ``public``; ``override``]
                (Some (``=>`` string_expression))
                :> MemberDeclarationSyntax
        ]

    let to_choice_class_internal fns (du: UnionType) (um: UnionMember) =
        let union_name = du.UnionClassNameWithTypeArgs
        let class_name = um.ChoiceClassName

        let members =
            fns
            |> Seq.collect (fun f -> f du um)

        ``class`` class_name ``<<`` [] ``>>``
            ``:`` (Some union_name) ``,`` [ ]
            [ ``public``; ``partial`` ]
            ``{``
                members
            ``}``
            :> MemberDeclarationSyntax

    let to_choice_class du um =
        let common_member_fns =
            [
                ctor
                value_property
                match_function_override
            ]

        let value_semantics_member_fns =
            [
                equals_override
                hashcode_override
                tostring_override
            ]

        let fns = common_member_fns @ value_semantics_member_fns
        to_choice_class_internal fns du um

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
        let choice_classes = du.UnionMembers |> Seq.map (UnionMemberClassDeclarationBuilder.to_choice_class du)
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

    let to_union_class_declaration du =
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
