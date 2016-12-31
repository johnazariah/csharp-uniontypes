namespace BrightSword.CSharpExtensions.DiscriminatedUnion

open System.Text.RegularExpressions
open System.IO

open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Syntax
open BrightSword.RoslynWrapper

open Utility.Monads.MaybeMonad

[<AutoOpen>]
module internal DeclarationBuilderCommon = 
    let toParameterName (str : string) =
        sprintf "%s%s" (str.Substring(0, 1).ToLower()) (str.Substring(1))

    let to_match_function_parameter_name = toParameterName >> sprintf "%sFunc"
    
    let to_match_function_parameters result_type du = 
        let to_match_function_parameter um = 
            let match_function_parameter_type = 
                um.MemberArgumentType 
                |> Option.fold (fun _ t -> sprintf "Func<%s, %s>" t.unapply result_type) 
                       (sprintf "Func<%s>" result_type)
            let match_function_parameter_name = um.MemberName.unapply |> to_match_function_parameter_name
            (match_function_parameter_name, ``type`` match_function_parameter_type)
        du.UnionMembers |> Seq.map to_match_function_parameter
    
    let to_match_function invocation du = 
        let parameters = to_match_function_parameters "TResult" du
        let override_or_abstract = (invocation |> Option.fold (fun _ _ -> ``override``) ``abstract``)
        [ 
            ``arrow_method`` "TResult" "Match" ``<<`` [ "TResult" ] ``>>`` ``(`` parameters ``)`` 
                [ ``public``; override_or_abstract ] 
                invocation :> MemberDeclarationSyntax
        ]
    
[<AutoOpen>]
module internal UnionMemberClassDeclarationBuilder = 
    let private pick_value_or_singleton fv fs um = 
       um.MemberArgumentType |> Option.fold fv fs

    let ctor _ um = 
        let ctor_value _ (t: FullTypeName) =
            let member_name = um.MemberName.unapply
            let arg_type_name = t.unapply 
            let assignment = statement (ident "Value" <-- ident "value")
            [
                ``constructor`` member_name ``(`` [ ("value", ``type`` arg_type_name) ] ``)`` 
                    ``:`` [] 
                    [ ``public`` ] 
                    ``{`` 
                        [ assignment ] 
                    ``}`` 
                    :> MemberDeclarationSyntax
            ]

        let ctor_singleton = []

        um 
        |> pick_value_or_singleton ctor_value ctor_singleton

    let value_property _ um = 
        let value_property_value _ (t: FullTypeName) = 
            let arg_type_name = t.unapply 
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

    let equals_override _ um = 
        let member_name = um.MemberName.unapply
        
        let equality_expression_builder base_expression _ = 
            let other_value_is_same = ``invoke`` (ident "Value.Equals") ``(`` [ ``((`` (``cast`` member_name (ident "other")) ``))`` <.> (ident "Value") ] ``)``
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
            base_expression <^> ``((`` ((ident "Value" <?.> ("GetHashCode", [])) <??> (``invoke`` (literal "null" <.> ident "GetHashCode") ``(`` [] ``)``)) ``))``
            :> ExpressionSyntax

        let get_hash_code_expression = 
            ``invoke`` (ident "GetType().FullName.GetHashCode") ``(`` [] ``)``
            :> ExpressionSyntax
        
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
            :> ExpressionSyntax
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

    let to_choice_class_internal fns (du: UnionType) um = 
        let union_name = du.unapply
        let member_name = um.MemberName.unapply

        let members = 
            fns
            |> Seq.collect (fun f -> f du um)

        ``class`` member_name ``<<`` [] ``>>`` 
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
module internal UnionTypeClassDeclarationBuilder = 
    let to_private_ctor (du : UnionType) = 
        let className = du.UnionTypeName.unapply
        [ 
            ``constructor`` className ``(`` [] ``)`` ``:`` [] 
                [ ``private`` ] 
                ``{`` 
                    [] 
                ``}`` 
                :> MemberDeclarationSyntax 
        ]
    
    let to_access_members (du : UnionType) =
        let union_name = du.unapply
        
        let to_access_member um = 
            let member_name = um.MemberName.unapply
            
            let to_access_member_method _ (t : FullTypeName) =
                let method_name = member_name |> sprintf "New%s"
                let method_arg_name = "value"
                let method_arg_type = t.unapply
                let initialization_expression = 
                    ``=>``(``new`` (``type`` [ "ChoiceTypes"; member_name ]) ``(`` [ ident method_arg_name ] ``)``)

                ``arrow_method`` union_name method_name ``<<`` [] ``>>`` ``(`` [ (method_arg_name, ``type`` method_arg_type) ] ``)`` 
                    [ ``public``; ``static`` ] 
                    (Some initialization_expression) 
                    :> MemberDeclarationSyntax
            
            let to_access_member_field = 
                let field_initializer = ``:=`` (``new`` (``type`` [ "ChoiceTypes"; member_name ]) ``(`` [] ``)``)
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
        let class_name = du.UnionTypeName.unapply
        let type_parameters = du.UnionTypeParameters |> Seq.map (fun p -> p.unapply)
        in 
        [
            ``arrow_method`` "bool" "Equals" ``<<`` [] ``>>`` ``(`` [ ("other", ``generic type`` class_name ``<<`` type_parameters ``>>``) ]``)``
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
        let class_name = du.UnionTypeName.unapply
        let type_parameters = du.UnionTypeParameters |> Seq.map (fun p -> p.unapply)
        let duType = ``generic type`` class_name ``<<`` type_parameters ``>>``
        [
            ``operator ==`` ("left", "right", duType)
                (``=>`` (((ident "left") <?.> ("Equals", [ident "right"])) <??> ``false``))
                :> MemberDeclarationSyntax
        ]

    //   public static bool operator !=(Maybe<T> left, Maybe<T> right) => !(left == right);
    let to_neq_operator du =
        let class_name = du.UnionTypeName.unapply
        let type_parameters = du.UnionTypeParameters |> Seq.map (fun p -> p.unapply)
        let duType = ``generic type`` class_name ``<<`` type_parameters ``>>``
        [
            ``operator !=`` ("left", "right", duType)
                (``=>`` (! ((ident "left") <==> (ident "right"))))

                :> MemberDeclarationSyntax
        ]

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
                to_private_ctor
                to_match_function_abstract
                to_access_members
                to_wrapper_type
                to_equatable_equals_method
                to_structural_equality_equals_method 
                to_structural_equality_gethashcode_method
                to_eq_operator
                to_neq_operator
            ]
        to_class_declaration_internal fns du

[<AutoOpen>]
module internal NamespaceDeclarationBuilder =     
    let to_namespace_declaration ns = 
        ``namespace`` ns.NamespaceName.unapply 
            ``{`` 
                (ns.Usings |> List.map (fun u -> u.unapply)) 
                (ns.Unions |> List.map to_class_declaration) 
            ``}``

[<AutoOpen>]
module internal CompilationUnitDeclarationBuilder =     
    let namespace_to_code namespace_declaration_syntax = 
        ``compilation unit`` 
            [ 
                namespace_declaration_syntax
            ] 
        |> generateCodeToString

module CodeGenerator =
    let generate_code_for_text text =
        text |> (parseTextToNamespace >> to_namespace_declaration >> namespace_to_code)

    let mapTuple2 f (a, b) = (f a, f b)
    let internal trimWS (s: System.String) = s.Trim()   
    let internal fixupNL t = Regex.Replace(t, "(?<!\r)\n", "\r\n")

    let ensure_input_file input_file = 
        input_file  |> Option.filter (fun f -> File.Exists f) |> Option.map FileInfo

    let ensure_output_directory output_file = 
        output_file |> Option.map (FileInfo >> (fun fi -> fi.Directory.Create (); fi))

    let generate_code_for_csunion_file (input_file, output_file) = 
        maybe {
            let! inputFileInfo = ensure_input_file input_file
            let! outputFileInfo = ensure_output_directory output_file
            let text = File.ReadAllText inputFileInfo.FullName
            let! ns = parse_namespace_from_text text
            let code = ns |> (to_namespace_declaration >> namespace_to_code >> trimWS >> fixupNL)
            do File.WriteAllText (outputFileInfo.FullName, code)
            return ()
        }

