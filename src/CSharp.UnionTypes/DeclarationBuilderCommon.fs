namespace CSharp.UnionTypes

open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Syntax
open BrightSword.RoslynWrapper

[<AutoOpen>]
module DeclarationBuilderCommon =
    let toParameterName (str : string) =
        sprintf "%s%s" (str.Substring(0, 1).ToLower()) (str.Substring(1))

    let to_match_function_parameter_name = toParameterName >> sprintf "%sFunc"

    let to_match_function_parameters result_type du =
        let to_match_function_parameter um =
            let match_function_parameter_type =
                um.MemberArgumentType
                |> Option.fold (fun _ t -> sprintf "Func<%s, %s>" t.CSharpTypeName result_type)
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
