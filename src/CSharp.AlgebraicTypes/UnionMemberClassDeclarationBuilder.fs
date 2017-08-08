namespace CSharp.UnionTypes

open Microsoft.CodeAnalysis.CSharp.Syntax
open BrightSword.RoslynWrapper

[<AutoOpen>]
module UnionMemberClassDeclarationBuilder =
    let private pick_value_or_singleton fv fs um =
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