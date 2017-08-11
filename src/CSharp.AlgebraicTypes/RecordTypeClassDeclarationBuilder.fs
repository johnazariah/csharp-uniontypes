namespace CSharp.AlgebraicTypes

open Microsoft.CodeAnalysis.CSharp.Syntax
open BrightSword.RoslynWrapper

module RecordTypeClassDeclarationBuilder =
    let to_class_declaration_internal fns (du : RecordType) =
        let className     = du.TypeDeclaration.SimpleTypeName        
        let typeArguments = du.TypeDeclaration.TypeParametersStringList
        let fullClassName = du.TypeDeclaration.FullTypeName
        
        let members = 
            fns 
            |> Seq.collect (fun f -> 
                du 
                |> (f >> List.toSeq))

        ``class`` className ``<<`` typeArguments ``>>``
            ``:`` None ``,`` [ sprintf "IEquatable<%s>" fullClassName; "IStructuralEquatable" ]
            [ ``public``; ``abstract``; ``partial`` ]
            ``{``
                members
            ``}``
            :> MemberDeclarationSyntax

    let to_class_declaration du =
        let fns =
            [
//                to_base_value
//                to_private_ctor
//                to_match_function_abstract
//                to_access_members
//                to_wrapper_type
//                to_equatable_equals_method
//                to_structural_equality_equals_method
//                to_structural_equality_gethashcode_method
//                to_eq_operator
//                to_neq_operator
//                to_base_cast
            ]
        to_class_declaration_internal fns du


