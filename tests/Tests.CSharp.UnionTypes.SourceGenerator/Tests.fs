module Tests

open CSharp.UnionTypes.SourceGenerator
open Xunit

[<Fact>]
let ``Maybe<T> is generated`` () =
    let actual =
        """namespace CSharp.UnionTypes.TestApplication
{
	union Maybe<T> { Some<T> | None };
}"""
        |> GenerateNamespaceCode

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
    Assert.Equal (expected, actual)