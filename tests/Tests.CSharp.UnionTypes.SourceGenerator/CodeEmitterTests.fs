module CSharp.UnionTypes.SourceGenerator.CodeEmitterTests

open CSharp.UnionTypes.CodeEmitter
open Xunit

[<Fact>]
let ``Maybe<T> is generated correctly`` () =
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
        public sealed partial record Some(T Value) : Maybe<T>
        {
            override public string ToString() => $"Some {Value}";
        }
        public sealed partial record None() : Maybe<T>
        {
            override public string ToString() => $"None";
        }
    }
}
"""
    Assert.Equal (expected.Replace("\r\n", "\n"), actual.Replace("\r\n", "\n"))