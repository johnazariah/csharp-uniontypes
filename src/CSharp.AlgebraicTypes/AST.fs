namespace CSharp.AlgebraicTypes
#if !true

[<AutoOpen>]
module AST =
    type Symbol = 
    | Symbol of string
    with 
        member this.unapply = match this with | Symbol s -> s
        override this.ToString() = this.unapply

    type DottedName = 
    | DottedName of string
    with 
        static member apply (components) =
            components 
            |> String.concat "."
            |> DottedName
        member this.unapply = match this with | DottedName s -> s
        override this.ToString() = this.unapply

    let private toPointedTypeParameterString ts =
        ts
        |> Seq.map (fun t -> t.ToString())
        |> String.concat ", "
        |> (fun t -> if t = "" then "" else (sprintf "<%s>" t))

    type TypeDeclaration = {
        TypeName : Symbol
        TypeArguments : (Symbol list) option
    }
    with
        static member apply(typeName, typeArguments) = { TypeName = typeName; TypeArguments = typeArguments }

        member this.SimpleTypeName = 
            this.TypeName.ToString()

        member this.TypeParametersStringList = 
            this.TypeArguments 
            |> Option.fold (fun _ tas -> 
                tas |> List.map (fun ta -> ta.ToString())) []

        member this.TypeParameterString =
            this.TypeArguments
            |> Option.fold (fun _ ts -> toPointedTypeParameterString ts) ""

        member this.FullTypeName = 
            sprintf "%s%s" this.SimpleTypeName this.TypeParameterString

        override this.ToString() = 
            this.FullTypeName

    type TypeReference = {
        TypeName : DottedName
        TypeParameters : (TypeReference list) option
    }
    with
        static member apply(typeName, typeParameters) = { TypeName = typeName; TypeParameters = typeParameters }

        member this.SimpleTypeName = 
            this.TypeName.ToString()

        member this.TypeParameterString =
            this.TypeParameters
            |> Option.fold (fun _ ts -> toPointedTypeParameterString ts) ""

        member this.FullTypeName = 
            sprintf "%s%s" this.SimpleTypeName this.TypeParameterString

        override this.ToString() = 
            this.FullTypeName
            

    type TypedTypeMember = {
        MemberName : Symbol
        MemberType : TypeReference
    }
    with
        static member apply(memberName, memberType) = { MemberName = memberName; MemberType = memberType }

        member this.ToUnionMemberString  = sprintf "%s<%s>"  (this.MemberName.ToString()) (this.MemberType.ToString())
        member this.ToRecordMemberString = sprintf "%s : %s" (this.MemberName.ToString()) (this.MemberType.ToString())

    type UnionTypeMember = 
    | UntypedMember of Symbol
    | TypedMember   of TypedTypeMember
    with
        member this.MemberName = 
            match this with
            | UntypedMember s -> s.ToString()
            | TypedMember   m -> m.MemberName.ToString()

        member this.ChoiceClassName = 
            sprintf "%sClass" this.MemberName

        member this.MemberAccessName = 
            match this with
            | UntypedMember s -> this.MemberName
            | TypedMember   m -> sprintf "New%s" this.MemberName
            
        override this.ToString() = 
            match this with
            | UntypedMember s -> s.ToString()
            | TypedMember   s -> s.ToUnionMemberString

    type UnionType = {
        TypeDeclaration : TypeDeclaration;
        TypeMembers     : UnionTypeMember list;
        BaseType        : TypeReference option
    }
    with
        static member apply((typeDeclaration, baseType), typeMembers) = {
            TypeDeclaration = typeDeclaration
            TypeMembers     = typeMembers
            BaseType        = baseType
        }
        override this.ToString() =
            let typeName = this.TypeDeclaration.ToString()

            let constrains = 
                this.BaseType 
                |> Option.fold (fun _ b -> sprintf " constrains %s" (b.ToString())) ""
            
            let members = 
                this.TypeMembers 
                |> Seq.map (fun m -> m.ToString())
                |> String.concat (" | ")
            
            sprintf @"union %s%s { %s }" typeName constrains members

    type RecordTypeMember =
    | TypedMember of TypedTypeMember
    with
        override this.ToString() = 
            match this with
            | TypedMember   s -> s.ToRecordMemberString

    type RecordType = {
        TypeDeclaration : TypeDeclaration;
        TypeMembers     : RecordTypeMember list;
    }
    with
        static member apply(typeDeclaration, typeMembers) = {
            TypeDeclaration = typeDeclaration
            TypeMembers     = typeMembers
        }
        override this.ToString() =
            let typeName = this.TypeDeclaration.ToString()

            let members = 
                this.TypeMembers 
                |> Seq.map (fun m -> m.ToString())
                |> String.concat ("; ")
            
            sprintf "record %s { %s }" typeName members

    type Using = 
    | Using of DottedName
    with
        member this.unapply = match this with | Using dn -> sprintf "using %s" (dn.ToString())
        override this.ToString() = this.unapply

    type NamespaceMember = 
    | Using  of Using
    | Union  of UnionType
    | Record of RecordType
    with 
        override this.ToString() = 
            match this with
            | Using  v -> v.ToString()
            | Union  v -> v.ToString()
            | Record v -> v.ToString()

    let IsUsing = function
    | NamespaceMember.Using u -> Some u
    | _ -> None

    let IsUnion= function
    | NamespaceMember.Union u -> Some u
    | _ -> None
    
    let IsRecord = function
    | NamespaceMember.Record r -> Some r
    | _ -> None

    type Namespace = {
        NamespaceName    : DottedName
        NamespaceMembers : NamespaceMember list
    }
    with
        static member apply(namespaceName, namespaceMembers) = {
            NamespaceName    = namespaceName
            NamespaceMembers = namespaceMembers
        }
        override this.ToString() = 
            let name = this.NamespaceName.ToString()

            let members = 
                this.NamespaceMembers
                |> Seq.map (fun m -> m.ToString())
                |> String.concat ("; ")

            sprintf  @"namespace %s { %s }" name members

        member this.Usings  = this.NamespaceMembers |> List.choose IsUsing
        member this.Unions  = this.NamespaceMembers |> List.choose IsUnion
        member this.Records = this.NamespaceMembers |> List.choose IsRecord
    
        
#else
[<AutoOpen>]
module AST =
    let toDottedName (head, (dotComponents : string list)) =
        let tail' = dotComponents |> String.concat "."

        let tail =
            if (tail' <> "") then ("." + tail')
            else ""
        sprintf "%s%s" head tail

    type Namespace =
        { NamespaceName : NamespaceName
          NamespaceMembers : NamespaceMember list }

        static member apply (name, members) =
            { NamespaceName = name
              NamespaceMembers = members }

        member this.Usings =
            let isUsing =
                function
                | Using x -> Some x
                | _ -> None
            this.NamespaceMembers |> List.choose isUsing

        member this.Unions =
            let isUnion =
                function
                | UnionType x -> Some x
                | _ -> None
            this.NamespaceMembers |> List.choose isUnion

        member this.Records =
            let isRecord =
                function
                | RecordType x -> Some x
                | _ -> None
            this.NamespaceMembers |> List.choose isRecord

        override this.ToString() =
            let members =
                seq {
                    yield! this.Usings  |> List.map (fun u -> u.unapply)
                    yield! this.Unions  |> List.map (fun u -> u.UnionClassNameWithTypeArgs)
                    yield! this.Records |> List.map (fun u -> u.RecordClassNameWithTypeArgs)
                }
                |> String.concat ("; ")
            sprintf @"namespace %s{%s}" this.NamespaceName.unapply members

    and NamespaceName =
        | NamespaceName of string
        static member apply = toDottedName >> NamespaceName

        member this.unapply =
            match this with
            | NamespaceName x -> x

        override this.ToString() = this.unapply

    and NamespaceMember =
        | Using of UsingName
        | UnionType of UnionType
        | RecordType of RecordType
        override this.ToString() =
            match this with
            | Using x -> x.ToString()
            | UnionType x -> x.ToString()
            | RecordType x -> x.ToString()

    and UsingName =
        | UsingName of string
        static member apply = toDottedName >> UsingName

        member this.unapply =
            match this with
            | UsingName x -> x

        override this.ToString() = this.unapply

    and RecordType = 
        { RecordTypeName : RecordTypeName
          RecordTypeParameters : TypeParameter list
          RecordMembers : RecordMember list }

        static member apply ((recordTypeName, typeArgumentListOption), recordMemberList) =
            { RecordTypeName = recordTypeName
              RecordMembers = recordMemberList
              RecordTypeParameters = typeArgumentListOption |> Option.fold (fun _ s -> s) [] }

        member this.RecordClassName = 
            this.RecordTypeName.unapply

        member this.RecordClassNameWithTypeArgs =
            let typeParameters =
                this.RecordTypeParameters
                |> Seq.map (fun a -> a.ToString())
                |> String.concat ", "
                |> (fun a ->
                if a <> "" then sprintf "<%s>" a
                else "")

            let bareTypeName = this.RecordTypeName.unapply
            in
            sprintf "%s%s" bareTypeName typeParameters

        override this.ToString() =
            let members =
                this.RecordMembers
                |> Seq.map (fun m -> m.ToString())
                |> String.concat "; "
            sprintf "record %s ::= [ %s ]" this.RecordClassNameWithTypeArgs members

    and RecordTypeName =
        | RecordTypeName of string

        member this.unapply =
            match this with
            | RecordTypeName x -> x

        override this.ToString() = this.unapply
    
    and RecordMember =
        { MemberName : RecordMemberName
          MemberArgumentType : FullTypeName }

        static member apply(memberName, typeArgument) =
            { MemberName = memberName
              MemberArgumentType = typeArgument }

        member this.RecordMemberAccessName = 
            this.MemberName.unapply

        override this.ToString() =
            sprintf "%s : %s" this.MemberName.unapply this.MemberArgumentType.CSharpTypeName

    and RecordMemberName =
        | RecordMemberName of string

        member this.unapply =
            match this with
            | RecordMemberName x -> x

        override this.ToString() = this.unapply

    and UnionType =
        { UnionTypeName : UnionTypeName
          UnionTypeParameters : TypeParameter list
          UnionMembers : UnionMember list
          BaseType : FullTypeName option }

        static member apply (((unionTypeName, typeArgumentListOption), baseType), unionMemberList) =
            { UnionTypeName = unionTypeName
              UnionMembers = unionMemberList
              UnionTypeParameters = typeArgumentListOption |> Option.fold (fun _ s -> s) []
              BaseType = baseType }

        member this.UnionClassName = 
            this.UnionTypeName.unapply

        member this.UnionClassNameWithTypeArgs =
            let typeParameters =
                this.UnionTypeParameters
                |> Seq.map (fun a -> a.ToString())
                |> String.concat ", "
                |> (fun a ->
                if a <> "" then sprintf "<%s>" a
                else "")

            let bareTypeName = this.UnionTypeName.unapply
            in
            sprintf "%s%s" bareTypeName typeParameters

        override this.ToString() =
            let members =
                this.UnionMembers
                |> Seq.map (fun m -> m.ToString())
                |> String.concat " | "
            sprintf "union %s ::= [ %s ]" this.UnionClassNameWithTypeArgs members

    and UnionTypeName =
        | UnionTypeName of string

        member this.unapply =
            match this with
            | UnionTypeName x -> x

        override this.ToString() = this.unapply

    and TypeParameter =
        | TypeArgument of string

        member this.unapply =
            match this with
            | TypeArgument x -> x

        override this.ToString() = this.unapply

    and UnionMember =
        { MemberName : UnionMemberName
          MemberArgumentType : FullTypeName option }

        static member apply(memberName, typeArgument) =
            { MemberName = memberName
              MemberArgumentType = typeArgument }

        member this.ChoiceClassName = 
            sprintf "%sClass" this.MemberName.unapply

        member this.UnionMemberAccessName = 
            this.MemberArgumentType 
            |> Option.fold (fun c _ -> sprintf "New%s" c) this.MemberName.unapply

        member this.ValueConstructor = this.UnionMemberAccessName

        override this.ToString() =
            this.MemberArgumentType
            |> Option.fold (fun _ s -> sprintf "%s of %s" this.MemberName.unapply (s.ToString()))
                   (sprintf "%s" this.MemberName.unapply)

    and UnionMemberName =
        | UnionMemberName of string

        member this.unapply =
            match this with
            | UnionMemberName x -> x

        override this.ToString() = this.unapply

    and FullTypeName =
        { FullyQualifiedTypeName : string
          TypeArguments : FullTypeName list }

        member this.CSharpTypeName = 
            let typeArguments' =
                this.TypeArguments
                |> Seq.map (fun ta -> ta.ToString ())
                |> String.concat ", "

            let typeArguments =
                if (typeArguments' <> "") then sprintf "<%s>" typeArguments'
                else ""

            sprintf "%s%s" this.FullyQualifiedTypeName typeArguments

        static member apply (nonGenericNamePart, typeArguments) =
            { FullyQualifiedTypeName = toDottedName nonGenericNamePart
              TypeArguments = typeArguments |> Option.fold (fun _ s -> s) [] }

        override this.ToString() = this.CSharpTypeName
#endif