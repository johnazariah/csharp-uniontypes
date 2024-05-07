namespace CSharp.UnionTypes

[<AutoOpen>]
module CodeEmitter =
    (*
        // union Maybe<T> { Some<T> | None }
        public abstract partial record Maybe<T>
        {
            private Maybe() { }

            public sealed partial record Some(T Value) : Maybe<T>;
            public sealed partial record None() : Maybe<T>;
        }
    *)

    let emitCodeForNamespace (ns : Namespace) : string =
        let generateUsing (using : UsingName) : string =
            $"using {using};"

        let generateUnion (union : UnionType) : string =
            let unionNameWithGenericArguments = ""
            let unionName = ""

            let generateUnionMember (unionMember : UnionMember) : string =
                let unionMemberName = ""
                $"public sealed partial record {unionMemberName} : {unionNameWithGenericArguments}"

            let unionMembers =
                union.UnionMembers
                |> Seq.map generateUnionMember
                |> Seq.reduce (fun res curr -> $"\t{res}\r\n{curr}")

            $"public abstract partial record {unionNameWithGenericArguments}
            {{
                private {unionName}() {{ }}

            {unionMembers}
            }}"

        let usings =
            ns.Usings
            |> Seq.map generateUsing
            |> Seq.map (fun s -> $"\t{s}")
            |> Seq.reduce (fun res curr -> $"\t{res}\r\n{curr}")

        let unions =
            ns.Unions
            |> Seq.map generateUnion
            |> Seq.reduce (fun res curr -> $"\t{res}\r\n{curr}")

        $"namespace {ns.NamespaceName}
        {{
        {usings}

        {unions}
        }}"


