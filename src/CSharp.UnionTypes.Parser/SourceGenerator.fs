namespace CSharp.UnionTypes

open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.Text
open System.Text
open System.IO
open System.Threading


module SourceGenerator =
    let GenerateNamespaceCode (text: string) =
        parseTextToNamespace text
        |> emitCodeForNamespace

    [<Generator>]
    type FileTransformGenerator() =
        interface IIncrementalGenerator with
            member _.Initialize (context : IncrementalGeneratorInitializationContext) =
                let fileIsCSUnion (text : AdditionalText) =
                    text.Path.EndsWith (".csunion")

                let generateUnion (text: AdditionalText) (cancellationToken: CancellationToken) =
                    let name =
                        Path.GetFileName text.Path
                    let code =
                        text.GetText cancellationToken
                        |> _.ToString()
                        |> GenerateNamespaceCode
                    in (name, code)

                let writeSourceFile (source : SourceProductionContext) ((name:string), (code:string)) =
                    source.AddSource ($"{name}.generated.cs", SourceText.From(code, Encoding.UTF8))

                let pipeline =
                    context
                        .AdditionalTextsProvider
                        .Where(fileIsCSUnion)
                        .Select(generateUnion)

                context.RegisterSourceOutput<string * string>(pipeline, writeSourceFile)
