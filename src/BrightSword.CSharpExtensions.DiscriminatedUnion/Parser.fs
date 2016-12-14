namespace BrightSword.CSharpExtensions.DiscriminatedUnion

open FParsec
open System

[<AutoOpen>]
module internal Parser = 
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
    let splitWords sep = sepBy1 word sep
    let comma = wstr ","
    let identifier = 
        spaces >>. regex "[\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Lm}_][\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Lm}\d_]*[?]?" .>> spaces
    let dotComponent : Parser<string, unit> = (pchar '.') >>. identifier .>> spaces
    let fullTypeName, fullTypeNameImpl = createParserForwardedToRef()
    let typeArguments = pointed (sepBy1 fullTypeName comma)
    let nonGenericNamePart = spaces >>. identifier .>>. many dotComponent
    
    do fullTypeNameImpl := nonGenericNamePart .>>. opt typeArguments |>> FullTypeName.apply
    
    let memberName = word |>> UnionMemberName
    let caseClassMember = 
        (wstr "case class" >>. memberName) .>>. (pointed fullTypeName) .>> wstr ";" |>> UnionMember.CaseClass 
        <?> "Case Class"
    let caseObjectMember = wstr "case object" >>. memberName .>> wstr ";" |>> UnionMember.CaseObject <?> "Case Object"
    let bracedMany p = braced (many p)
    let bracedMany1 p = braced (many1 p)
    let caseMember = caseClassMember <|> caseObjectMember
    let caseMembers = (bracedMany1 caseMember) .>> opt (wstr ";")
    let typeParameters = pointed (splitWords comma) |>> List.map TypeArgument
    let unionTypeName = word |>> UnionTypeName
    let unionType = 
        (wstr "union" >>. unionTypeName) .>>. (opt typeParameters) .>>. opt ((wstr "constrains") >>. fullTypeName) 
        .>>. caseMembers |>> (UnionType.apply >> UnionType) <?> "Union"
    let namespaceName = word |>> NamespaceName
    let bracedNamedBlock blockTag blockNameParser blockItemParser = 
        (wstr blockTag >>. blockNameParser) .>>. bracedMany blockItemParser
    let usingName = nonGenericNamePart |>> UsingName.apply
    let using = wstr "using" >>. usingName .>> wstr ";" |>> Using <?> "Using"
    let namespaceMember = using <|> unionType
    let ``namespace`` = bracedNamedBlock "namespace" namespaceName namespaceMember |>> Namespace.apply <?> "Namespace"

    let parseTextToNamespace str = 
        match run ``namespace`` str with
        | Success(result, _, _) -> 
            Some result
        | Failure(err, _, _) -> 
            printfn "Failure:%s[%s]" str err
            None
