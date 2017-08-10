namespace CSharp.AlgebraicTypes

open Microsoft.CodeAnalysis.CSharp.Syntax
open BrightSword.RoslynWrapper

[<AutoOpen>]
module internal DeclarationBuilderCommon =
    let toParameterName (str : string) =
        sprintf "%s%s" (str.Substring(0, 1).ToLower()) (str.Substring(1))

    let to_match_function_parameter_name = toParameterName >> sprintf "%sFunc"

    let to_match_function_parameters result_type (du : UnionType) =
        let to_match_function_parameter um =
            let match_function_parameter_type =
                match um with 
                | UnionTypeMember.UntypedMember _  -> (sprintf "Func<%s>" result_type)
                | UnionTypeMember.TypedMember   tm -> (sprintf "Func<%s, %s>" tm.MemberType.FullTypeName result_type)
                       
            let match_function_parameter_name = um.MemberName |> to_match_function_parameter_name
            (match_function_parameter_name, ``type`` match_function_parameter_type)
        
        du.TypeMembers |> Seq.map to_match_function_parameter

    let to_match_function invocation du =
        let parameters = to_match_function_parameters "TResult" du
        let override_or_abstract = (invocation |> Option.fold (fun _ _ -> ``override``) ``abstract``)
        [
            ``arrow_method`` "TResult" "Match" ``<<`` [ "TResult" ] ``>>`` ``(`` parameters ``)``
                [ ``public``; override_or_abstract ]
                invocation :> MemberDeclarationSyntax
        ]
