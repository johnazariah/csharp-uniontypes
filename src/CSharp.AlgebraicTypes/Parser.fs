namespace CSharp.AlgebraicTypes

open FParsec

[<AutoOpen>]
module Parser =
    let (<!>) (p: Parser<_,_>) label (stream: CharStream<_>) =
        printfn "%A: Entering %s" stream.Position label
        let reply = p stream
        printfn "%A: Leaving %s (%A)" stream.Position label reply.Status
        reply

    let ws p = spaces >>. p .>> spaces

    let wchar   p  = attempt (ws (pchar p))
    let wstr    p  = attempt (ws (pstring p))
                   
    let braced  p  = between (wstr "{") (wstr "}") p
    let pointed p  = between (wstr "<") (wstr ">") p
                   
    let dot        = pstring "."
    let comma      = pstring ","
    let pipe       = pstring "|"
    let semicolon  = pstring ";"
    let colon      = pstring ":"

    //let identifier = regex "[\p{L}_]([\p{L}\p{Nd}_]*)" 
    let identifier = ws (regex "[\p{L}_][\p{L}\p{Nd}\d_]*[?]?")
    
    // Symbol
    let symbol = 
        ws identifier
        |>> Symbol.Symbol
        <!> "Symbol"

    // DottedName
    let dottedName = 
        sepBy1 identifier dot
        |>> DottedName.apply
        <!> "DottedName"

    // TypeDeclaration 
    let typeDeclaration =
        let typeArgs = pointed (sepBy symbol comma)
        (symbol .>>. opt typeArgs)
        |>> TypeDeclaration.apply
        <!> "TypeDeclaration"

    // TypeReference
    let typeReference, typeReferenceImpl = createParserForwardedToRef ()
    let typeParams = pointed (sepBy typeReference comma)

    do typeReferenceImpl :=
        (dottedName .>>. opt typeParams) |>> TypeReference.apply
        <!> "TypeReference"

    // UnionTypeMember
    let unionTypeMember =
        // UnionTypeTypedMember 
        let unionTypeTypedMember =
            symbol .>>. pointed (typeReference) 
            |>> TypedTypeMember.apply 
            |>> UnionTypeMember.TypedMember
            <!> "UnionTypeTypedMember"
    
        // UnionTypeUntypedMember 
        let unionTypeUntypedMember = 
            symbol 
            |>> UnionTypeMember.UntypedMember
            <!> "UnionTypeUntypedMember"
         
        choice [
            attempt unionTypeTypedMember 
            unionTypeUntypedMember
        ]
        <!> "UnionTypeMember"

    // UnionType
    let unionType = 
        let constrainsOpt = opt (wstr "constrains" >>. typeReference)        
        wstr "union" 
            >>. typeDeclaration 
            .>>. constrainsOpt 
            .>>. braced (sepBy1 unionTypeMember pipe) 
        |>> UnionType.apply
        <!> "UnionType"

    // RecordTypeMember
    let recordTypeMember =
        symbol .>> colon .>>. typeReference
        |>> TypedTypeMember.apply 
        |>> RecordTypeMember.TypedMember
        <!> "RecordTypeMember"

    // RecordType
    let recordType =
        wstr "record"
            >>. typeDeclaration 
            .>>. braced (sepBy1 recordTypeMember semicolon) 
        |>> RecordType.apply
        <!> "RecordType"

    // Using
    let using = 
        wstr "using" >>. dottedName .>> semicolon
        |>> Using.Using
        <!> "Using"

    // NamespaceMember
    let namespaceMember = 
        choice [
            attempt unionType   .>> opt semicolon |>> NamespaceMember.Union
            attempt recordType  .>> opt semicolon |>> NamespaceMember.Record
            attempt using       .>> opt semicolon |>> NamespaceMember.Using
        ]

    // Namespace
    let ``namespace`` = 
        wstr "namespace" >>. dottedName .>>. braced (many namespaceMember)
        |>> Namespace.apply

    let parseTextToNamespace str =
        match run ``namespace`` str with
        | Success(result, _, _) -> result
        | Failure(err, _, _) -> sprintf "Failure:%s[%s]" str err |> failwith

    let parse_namespace_from_text str =
        match run ``namespace`` str with
        | Success(result, _, _) -> Some result
        | _ -> None
