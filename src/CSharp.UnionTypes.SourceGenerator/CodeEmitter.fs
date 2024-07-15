namespace CSharp.UnionTypes

[<AutoOpen>]
module CodeEmitter =
    open System.CodeDom.Compiler

    let generateCodeForNamespace (ns : Namespace) : string =
        let indentWriter : IndentedTextWriter  = new IndentedTextWriter(new System.IO.StringWriter ())

        let indentAndWriteLine (str : string) =
            indentWriter.Indent <- indentWriter.Indent + 1
            indentWriter.WriteLine str
            indentWriter.Indent <- indentWriter.Indent - 1

        let generateUsing (using : UsingName) =
            indentAndWriteLine $"using {using};"

        let generateUnion (union : UnionType) =
            let generateUnionMember (unionMember : UnionMember) =
                indentAndWriteLine $"public sealed partial record {unionMember.MemberName.unapply}{unionMember.UnionMemberValueMember} : {union.UnionClassNameWithTypeArgs}"
                indentAndWriteLine $"{{"
                indentWriter.Indent <- indentWriter.Indent + 1
                let memberValuePattern =
                    match unionMember.MemberArgumentType with
                    | Some _ -> $" {{Value}}"
                    | None -> ""
                indentAndWriteLine $"override public string ToString() => $\"{union.UnionClassNameWithTypeofTypeArgs}.{unionMember.MemberName.unapply}{memberValuePattern}\";"
                indentWriter.Indent <- indentWriter.Indent - 1
                indentAndWriteLine $"}}"

            indentAndWriteLine $"public abstract partial record {union.UnionClassNameWithTypeArgs}"
            indentAndWriteLine $"{{"

            indentWriter.Indent <- indentWriter.Indent + 1

            indentAndWriteLine $"private {union.UnionClassName}() {{ }}"

            union.UnionMembers
            |> Seq.iter generateUnionMember

            indentWriter.Indent <- indentWriter.Indent - 1

            indentAndWriteLine $"}}"

        indentWriter.WriteLine  $"namespace {ns.NamespaceName}"
        indentWriter.WriteLine  $"{{"

        ns.Usings |> Seq.iter generateUsing
        ns.Unions |> Seq.iter generateUnion

        indentWriter.WriteLine  $"}}"

        indentWriter.InnerWriter.ToString();

    let GenerateNamespaceCode (text: string) =
        parseTextToNamespace text
        |> generateCodeForNamespace

