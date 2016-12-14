namespace BrightSword.CSharpExtensions.DiscriminatedUnion

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
        
        override this.ToString() = 
            let members = 
                seq { 
                    yield! this.Usings |> List.map (fun u -> u.unapply)
                    yield! this.Unions |> List.map (fun u -> u.unapply)
                }
                |> String.concat ("; ")
            sprintf @"namespace %s{%s}" this.NamespaceName.unapply members
    
    and NamespaceName = 
        | NamespaceName of string
        
        member this.unapply = 
            match this with
            | NamespaceName x -> x
        
        override this.ToString() = this.unapply
    
    and NamespaceMember = 
        | Using of UsingName
        | UnionType of UnionType
        override this.ToString() = 
            match this with
            | Using x -> x.ToString()
            | UnionType x -> x.ToString()
    
    and UsingName = 
        | UsingName of string
        static member apply = toDottedName >> UsingName
        
        member this.unapply = 
            match this with
            | UsingName x -> x
        
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
        
        member this.unapply = 
            let typeParameters = 
                this.UnionTypeParameters
                |> Seq.map (fun a -> a.ToString())
                |> String.concat ", "
                |> (fun a -> 
                if a <> "" then sprintf "<%s>" a
                else "")
            
            let bareTypeName = this.UnionTypeName.unapply
            sprintf "%s%s" bareTypeName typeParameters
        
        override this.ToString() = 
            let members = 
                this.UnionMembers
                |> Seq.map (fun m -> m.ToString())
                |> String.concat " | "
            sprintf "union %s ::= [ %s ]" this.unapply members
    
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
        
        static member CaseClass(memberName, typeArgument) = 
            { MemberName = memberName
              MemberArgumentType = Some typeArgument }
        
        static member CaseObject(memberName) = 
            { MemberName = memberName
              MemberArgumentType = None }
        
        override this.ToString() = 
            this.MemberArgumentType 
            |> Option.fold (fun _ s -> sprintf "%s of %s" this.MemberName.unapply s.unapply) 
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
        
        static member apply (nonGenericNamePart, typeArguments) = 
            { FullyQualifiedTypeName = toDottedName nonGenericNamePart
              TypeArguments = typeArguments |> Option.fold (fun _ s -> s) [] }
        
        member this.unapply = 
            let typeArguments' = 
                this.TypeArguments
                |> Seq.map (fun ta -> ta.unapply)
                |> String.concat ", "
            
            let typeArguments = 
                if (typeArguments' <> "") then sprintf "<%s>" typeArguments'
                else ""
            
            sprintf "%s%s" this.FullyQualifiedTypeName typeArguments
        
        override this.ToString() = this.unapply
