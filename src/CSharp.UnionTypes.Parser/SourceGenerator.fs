namespace CSharp.UnionTypes

open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.Text
open System.Text
open System.IO
open System.Threading

module SourceGenerator =
    [<Generator>]
    type FileTransformGenerator() =
        interface IIncrementalGenerator with
            member _.Initialize (context : IncrementalGeneratorInitializationContext) =
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
                        .Where(fun text -> text.Path.EndsWith (".csunion"))
                        .Select(generateUnion)

                context.RegisterSourceOutput (pipeline, writeSourceFile)
