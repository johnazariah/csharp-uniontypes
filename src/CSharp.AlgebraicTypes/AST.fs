namespace CSharp.AlgebraicTypes

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
            | UntypedMember _ -> this.MemberName
            | TypedMember   _ -> sprintf "New%s" this.MemberName
            
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
