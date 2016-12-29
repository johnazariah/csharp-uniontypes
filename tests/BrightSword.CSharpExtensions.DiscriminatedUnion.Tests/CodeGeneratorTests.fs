namespace BrightSword.CSharpExtensions.DiscriminatedUnion.Tests

open BrightSword.CSharpExtensions.DiscriminatedUnion.AST
open BrightSword.CSharpExtensions.DiscriminatedUnion.UnionTypeCodeGenerator
open BrightSword.CSharpExtensions.DiscriminatedUnion.ChoiceClassCodeGenerator
open BrightSword.RoslynWrapper
open NUnit.Framework
open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Syntax
open System.Text.RegularExpressions

type SF = SyntaxFactory

module CodeGeneratorTests = 
    let maybe_of_T = 
        { UnionTypeName = UnionTypeName "Maybe"
          UnionTypeParameters = [ TypeArgument "T" ]
          UnionMembers = 
              [ { MemberName = UnionMemberName "None"
                  MemberArgumentType = None }
                { MemberName = UnionMemberName "Some"
                  MemberArgumentType = 
                      Some { FullyQualifiedTypeName = "T"
                             TypeArguments = [] } } ]
          BaseType = None }
    
    let class_to_code class_declaration_syntax = 
        ``compilation unit`` 
            [ ``namespace`` "DU.Tests" ``{`` [] [ class_declaration_syntax ] ``}`` :> MemberDeclarationSyntax ] 
        |> generateCodeToString
    
    let private test_code_gen generator expected = 
        let actual = 
            maybe_of_T
            |> to_class_declaration_internal [ generator ]
            |> class_to_code
        Assert.AreEqual(Regex.Replace(expected, "(?<!\r)\n", "\r\n"), Regex.Replace(actual, "(?<!\r)\n", "\r\n"))

    let private test_code_gen_choice generator expected = 
        let actual = 
            maybe_of_T.UnionMembers
            |> List.map ((to_choice_class_internal [ generator] maybe_of_T) >> class_to_code)
            |> String.concat("\n")
        Assert.AreEqual(Regex.Replace(expected, "(?<!\r)\n", "\r\n"), Regex.Replace(actual, "(?<!\r)\n", "\r\n"))
    
    [<Test>]
    let ``code-gen: private constructor``() = 
        let expected = @"namespace DU.Tests
{
    using System;

    public abstract partial class Maybe<T>
    {
        private Maybe()
        {
        }
    }
}"
        test_code_gen to_private_ctor expected
    
    [<Test>]
    let ``code-gen: match function abstract``() = 
        let expected = @"namespace DU.Tests
{
    using System;

    public abstract partial class Maybe<T>
    {
        public abstract TResult Match<TResult>(Func<TResult> noneFunc, Func<T, TResult> someFunc);
    }
}"
        test_code_gen to_match_function_abstract expected
    
    [<Test>]
    let ``code-gen: access members``() = 
        let expected = @"namespace DU.Tests
{
    using System;

    public abstract partial class Maybe<T>
    {
        public static readonly Maybe<T> None = new ChoiceTypes.None();
        public static Maybe<T> NewSome(T value) => new ChoiceTypes.Some(value);
    }
}"
        test_code_gen to_access_members expected
    
    [<Test>]
    let ``code-gen: equatable equals``() =
        let expected = @"namespace DU.Tests
{
    using System;

    public abstract partial class Maybe<T>
    {
        public bool Equals(Maybe<T> other) => Equals(other as object);
    }
}"
        test_code_gen to_equatable_equals_method expected 

    [<Test>]
    let ``code-gen: structuralequality equals``() =
        let expected = @"namespace DU.Tests
{
    using System;

    public abstract partial class Maybe<T>
    {
        public bool Equals(object other, IEqualityComparer comparer) => Equals(other);
    }
}"
        test_code_gen to_structural_equality_equals_method expected 

    [<Test>]
    let ``code-gen: structuralequality gethashcode``() =
        let expected = @"namespace DU.Tests
{
    using System;

    public abstract partial class Maybe<T>
    {
        public int GetHashCode(IEqualityComparer comparer) => GetHashCode();
    }
}"
        test_code_gen to_structural_equality_gethashcode_method expected 

    [<Test>]
    let ``code-gen: eq operator``() =
        let expected = @"namespace DU.Tests
{
    using System;

    public abstract partial class Maybe<T>
    {
        public static bool operator ==(Maybe<T> left, Maybe<T> right) => left?.Equals(right) ?? false;
    }
}"
        test_code_gen to_eq_operator expected 


    [<Test>]
    let ``code-gen: neq operator``() =
        let expected = @"namespace DU.Tests
{
    using System;

    public abstract partial class Maybe<T>
    {
        public static bool operator !=(Maybe<T> left, Maybe<T> right) => !(left == right);
    }
}"
        test_code_gen to_neq_operator expected 

    [<Test>]
    let ``code-gen-choice : equals_object_override``() =
        let expected = @"namespace DU.Tests
{
    using System;

    public class None : Maybe<T>, IEquatable<None>
    {
        public override bool Equals(object other) => this.Equals(other as None);
    }
}
namespace DU.Tests
{
    using System;

    public class Some : Maybe<T>, IEquatable<Some>
    {
        public override bool Equals(object other) => this.Equals(other as Some);
    }
}"
        test_code_gen_choice equals_object_override expected
    
    [<Test>]
    let ``code-gen: wrapper type``() = 
        let expected = @"namespace DU.Tests
{
    using System;

    public abstract partial class Maybe<T>
    {
        private static class ChoiceTypes
        {
            public class None : Maybe<T>, IEquatable<None>
            {
                public override TResult Match<TResult>(Func<TResult> noneFunc, Func<T, TResult> someFunc) => noneFunc();
                public override bool Equals(object other) => this.Equals(other as None);
            }

            public class Some : Maybe<T>, IEquatable<Some>
            {
                public Some(T value)
                {
                    Value = value;
                }

                private T Value
                {
                    get;
                }

                public override TResult Match<TResult>(Func<TResult> noneFunc, Func<T, TResult> someFunc) => someFunc(Value);
                public override bool Equals(object other) => this.Equals(other as Some);
            }
        }
    }
}"
        test_code_gen to_wrapper_type expected

    let COMPLETE_EXPECTED = @"namespace DU.Tests
{
    using System;

    public abstract partial class Maybe<T>
    {
        private Maybe()
        {
        }

        public abstract TResult Match<TResult>(Func<TResult> noneFunc, Func<T, TResult> someFunc);
        public static readonly Maybe<T> None = new ChoiceTypes.None();
        public static Maybe<T> NewSome(T value) => new ChoiceTypes.Some(value);
        private static class ChoiceTypes
        {
            public class None : Maybe<T>, IEquatable<None>
            {
                public override TResult Match<TResult>(Func<TResult> noneFunc, Func<T, TResult> someFunc) => noneFunc();
                public override bool Equals(object other) => this.Equals(other as None);
            }

            public class Some : Maybe<T>, IEquatable<Some>
            {
                public Some(T value)
                {
                    Value = value;
                }

                private T Value
                {
                    get;
                }

                public override TResult Match<TResult>(Func<TResult> noneFunc, Func<T, TResult> someFunc) => someFunc(Value);
                public override bool Equals(object other) => this.Equals(other as Some);
            }
        }

        public bool Equals(Maybe<T> other) => Equals(other as object);
        public bool Equals(object other, IEqualityComparer comparer) => Equals(other);
        public int GetHashCode(IEqualityComparer comparer) => GetHashCode();
        public static bool operator ==(Maybe<T> left, Maybe<T> right) => left?.Equals(right) ?? false;
        public static bool operator !=(Maybe<T> left, Maybe<T> right) => !(left == right);
    }
}"
    
    [<Test>]
    let ``code-gen: complete``() = 
        let actual = 
            maybe_of_T
            |> to_class_declaration
            |> class_to_code

        Assert.AreEqual(Regex.Replace(COMPLETE_EXPECTED, "(?<!\r)\n", "\r\n"), Regex.Replace(actual, "(?<!\r)\n", "\r\n"))
