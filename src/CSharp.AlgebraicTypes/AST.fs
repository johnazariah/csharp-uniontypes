namespace CSharp.UnionTypes

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
                    yield! this.Usings |> List.map (fun u -> u.unapply)
                    yield! this.Unions |> List.map (fun u -> u.UnionClassNameWithTypeArgs)
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

    and RecordTypeName =
        | RecordTypeName of string

        member this.unapply =
            match this with
            | RecordTypeName x -> x

        override this.ToString() = this.unapply
    
    and RecordMember =
        { MemberName : RecordMemberName
          MemberArgumentType : FullTypeName option }

        static member apply(memberName, typeArgument) =
            { MemberName = memberName
              MemberArgumentType = typeArgument }

        member this.ChoiceClassName = 
            sprintf "%sClass" this.MemberName.unapply

        member this.RecordMemberAccessName = 
            this.MemberArgumentType 
            |> Option.fold (fun c _ -> sprintf "New%s" c) this.MemberName.unapply

        member this.ValueConstructor = this.RecordMemberAccessName

        override this.ToString() =
            this.MemberArgumentType
            |> Option.fold (fun _ s -> sprintf "%s of %s" this.MemberName.unapply (s.ToString()))
                   (sprintf "%s" this.MemberName.unapply)

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