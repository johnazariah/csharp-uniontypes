namespace CSharp.UnionTypes.Tests

open System.IO
open System.Text.RegularExpressions

open CSharp.UnionTypes.Parser
open CSharp.UnionTypes.CodeGenerator

open NUnit.Framework

module IntegratedTests =

    let maybe_of_T = @"
namespace DU.Tests
{
    union Maybe<T> { None | Some<T> }
    union TrafficLights { Red | Amber | Green }
}"

    [<Test>]
    let ``code-gen from text: maybe``() =
        let actual = generate_code_for_text maybe_of_T
        CodeGeneratorTests.text_matches (CodeGeneratorTests.COMPLETE_EXPECTED, actual)

    [<Test>]
    let ``code-gen from file: maybe``() =
        let input_file = FileInfo("maybe.csunion")
        Assert.IsTrue <| File.Exists (input_file.FullName)

        let input = File.ReadAllText input_file.FullName
        CodeGeneratorTests.text_matches (maybe_of_T, input)

        let output_file = Path.Combine(input_file.Directory.FullName, "maybe.cs") |> FileInfo
        Assert.IsTrue <| Directory.Exists output_file.Directory.FullName

        File.Delete output_file.FullName
        Assert.IsFalse <| File.Exists (output_file.FullName)

        generate_code_for_csunion_file (Some input_file.FullName, Some output_file.FullName) |> ignore

        Assert.IsTrue <| File.Exists (output_file.FullName)

        let actual = File.ReadAllText output_file.FullName
        CodeGeneratorTests.text_matches (CodeGeneratorTests.COMPLETE_EXPECTED, actual)
