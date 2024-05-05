namespace CSharp.UnionTypes

open FParsec
open System

[<AutoOpen>]
module Parser =
    let ws p = spaces >>. p .>> spaces
    let word : Parser<string, unit> = ws (many1Chars asciiLetter)

    let wchar : char -> Parser<char, unit> =
        pchar
        >> ws
        >> attempt

    let wstr : string -> Parser<string, unit> =
        pstring
        >> ws
        >> attempt

    let braced p = wstr "{" >>. p .>> wstr "}"
    let pointed p = wstr "<" >>. p .>> wstr ">"
    let comma = wstr ","
    let pipe = wstr "|"
    let identifier =
        spaces >>. regex "[\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Lm}_][\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Lm}\d_]*[?]?" .>> spaces
    let dotComponent : Parser<string, unit> = (pchar '.') >>. identifier .>> spaces
    let fullTypeName, fullTypeNameImpl = createParserForwardedToRef()
    let typeArguments = pointed (sepBy1 fullTypeName comma)
    let dottedName = spaces >>. identifier .>>. many dotComponent

    do fullTypeNameImpl := dottedName .>>. opt typeArguments |>> FullTypeName.apply

    let memberName = word |>> UnionMemberName
    let caseMemberArgOpt = pointed fullTypeName |> opt
    let caseMember = ((memberName .>>. caseMemberArgOpt) |> ws) |>> UnionMember.apply
    let caseMembers = sepBy1 caseMember pipe
    let caseMembersBlock = braced caseMembers
    let typeParameters = (sepBy1 word comma |> pointed) |>> List.map TypeArgument
    let constrainsOpt = ((wstr "constrains") >>. fullTypeName) |> opt
    let unionTypeName = word |>> UnionTypeName
    let unionType =
        (wstr "union" >>. unionTypeName) .>>. (opt typeParameters) .>>. constrainsOpt .>>. caseMembersBlock
        .>> opt (wstr ";") |>> (UnionType.apply >> UnionType) <?> "Union"
    let usingName = dottedName |>> UsingName.apply
    let using = wstr "using" >>. usingName .>> wstr ";" |>> Using <?> "Using"
    let namespaceName = dottedName |>> NamespaceName.apply
    let namespaceMember = using <|> unionType
    let ``namespace`` =
        wstr "namespace" >>. namespaceName .>>. (namespaceMember |> (many >> braced)) |>> Namespace.apply
        <?> "Namespace"

    let parseTextToNamespace str =
        match run ``namespace`` str with
        | Success(result, _, _) -> result
        | Failure(err, _, _) -> sprintf "Failure:%s[%s]" str err |> failwith

    let parse_namespace_from_text str =
        match run ``namespace`` str with
        | Success(result, _, _) -> Some result
        | _ -> None
