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
    let identifier = ws (regex "[\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Lm}_][\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Lm}\d_]*[?]?")
    let dotComponent : Parser<string, unit> = (pchar '.') >>. identifier .>> spaces
    let fullTypeName, fullTypeNameImpl = createParserForwardedToRef()
    let typeArguments = pointed (sepBy1 fullTypeName comma)
    let dottedName = spaces >>. identifier .>>. many dotComponent

    do fullTypeNameImpl := dottedName .>>. opt typeArguments |>> FullTypeName.apply
    
    let typeParameters = (sepBy1 word comma |> pointed) |>> List.map TypeArgument
    let constrainsOpt = ((wstr "constrains") >>. fullTypeName) |> opt

    let unionMemberName = word |>> UnionMemberName
    let unionMemberArgOpt = pointed fullTypeName |> opt
    let unionMember = ((unionMemberName .>>. unionMemberArgOpt) |> ws) |>> UnionMember.apply
    let unionMembers = sepBy1 unionMember (wstr "|")
    let unionMembersBlock = braced unionMembers
    let unionTypeName = word |>> UnionTypeName
    let unionType =
        (wstr "union" >>. unionTypeName) .>>. (opt typeParameters) .>>. constrainsOpt .>>. unionMembersBlock
        .>> opt (wstr ";") |>> (UnionType.apply >> UnionType) <?> "Union"
    
    let recordMemberName = word |>> RecordMemberName
    let recordMemberArgOpt = pointed fullTypeName |> opt
    let recordMember = ((recordMemberName .>>. recordMemberArgOpt) |> ws) |>> RecordMember.apply
    let recordMembers = sepBy1 recordMember (wstr ";")
    let recordMembersBlock = braced recordMembers
    let recordTypeName = word |>> RecordTypeName
    let recordType =
        (wstr "record" >>. recordTypeName) .>>. (opt typeParameters) .>>. recordMembersBlock
        .>> opt (wstr ";") |>> (RecordType.apply >> RecordType) <?> "Record"

    let usingName = dottedName |>> UsingName.apply
    let using = wstr "using" >>. usingName .>> wstr ";" |>> Using <?> "Using"
 
    let namespaceName = dottedName |>> NamespaceName.apply
    let namespaceMember = using <|> unionType <|> recordType
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
