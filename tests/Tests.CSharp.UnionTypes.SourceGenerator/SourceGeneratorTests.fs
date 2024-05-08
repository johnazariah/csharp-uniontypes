module Tests.CSharp.UnionTypes.SourceGenerator.SourceGeneratorTests
open CSharp.UnionTypes.SourceGenerator
open Xunit

[<Fact>]
let ``Maybe<T> is generated correctly`` () =
    let input =
        """namespace CSharp.UnionTypes.TestApplication
{
	union Maybe<T> { Some<T> | None };
}"""

    let expected =
        """namespace CSharp.UnionTypes.TestApplication
{
    public abstract partial record Maybe<T>
    {
        private Maybe() { }
        public sealed partial record Some<T> : Maybe<T>;
        public sealed partial record None : Maybe<T>;
    }
}
"""

(*

            var compilation = CSharpCompilation.Create("foo", new SyntaxTree[] { syntaxTree }, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            // TODO: Uncomment this line if you want to fail tests when the injected program isn't valid _before_ running generators
            // var compileDiagnostics = compilation.GetDiagnostics();
            // Assert.False(compileDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error), "Failed: " + compileDiagnostics.FirstOrDefault()?.GetMessage());

            ISourceGenerator generator = new Generator();

            var driver = CSharpGeneratorDriver.Create(generator);
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generateDiagnostics);
            Assert.False(generateDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error), "Failed: " + generateDiagnostics.FirstOrDefault()?.GetMessage());

            string actual = outputCompilation.SyntaxTrees.Last().ToString();
*)

    let actual = ""

    Assert.Equal (expected, actual)