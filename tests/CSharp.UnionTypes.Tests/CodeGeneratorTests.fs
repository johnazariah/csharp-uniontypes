namespace CSharp.UnionTypes.Tests

open CSharp.UnionTypes.AST
open CSharp.UnionTypes.UnionMemberClassDeclarationBuilder
open CSharp.UnionTypes.UnionTypeClassDeclarationBuilder
open CSharp.UnionTypes.CodeGenerator
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
            [
                ``namespace`` "DU.Tests"
                    ``{``
                        [ "System"; "System.Collections" ]
                        [ class_declaration_syntax ]
                    ``}`` :> MemberDeclarationSyntax
            ]
        |> generateCodeToString

    let text_matches = (mapTuple2 (fixupNL >> trimWS) >> Assert.AreEqual)

    let private test_code_gen generator expected =
        let actual =
            maybe_of_T
            |> to_class_declaration_internal [ generator ]
            |> class_to_code
        text_matches (expected, actual)

    let private test_code_gen_choice generator expected =
        let actual =
            maybe_of_T.UnionMembers
            |> List.map ((to_choice_class_internal [ generator] maybe_of_T) >> class_to_code)
            |> String.concat("\n")
        text_matches (expected, actual)

    [<Test>]
    let ``code-gen: private constructor``() =
        let expected = @"namespace DU.Tests
{
    using System;
    using System.Collections;

    public abstract partial class Maybe<T> : IEquatable<Maybe<T>>, IStructuralEquatable
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
    using System.Collections;

    public abstract partial class Maybe<T> : IEquatable<Maybe<T>>, IStructuralEquatable
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
    using System.Collections;

    public abstract partial class Maybe<T> : IEquatable<Maybe<T>>, IStructuralEquatable
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
    using System.Collections;

    public abstract partial class Maybe<T> : IEquatable<Maybe<T>>, IStructuralEquatable
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
    using System.Collections;

    public abstract partial class Maybe<T> : IEquatable<Maybe<T>>, IStructuralEquatable
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
    using System.Collections;

    public abstract partial class Maybe<T> : IEquatable<Maybe<T>>, IStructuralEquatable
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
    using System.Collections;

    public abstract partial class Maybe<T> : IEquatable<Maybe<T>>, IStructuralEquatable
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
    using System.Collections;

    public abstract partial class Maybe<T> : IEquatable<Maybe<T>>, IStructuralEquatable
    {
        public static bool operator !=(Maybe<T> left, Maybe<T> right) => !(left == right);
    }
}"
        test_code_gen to_neq_operator expected

    [<Test>]
    let ``code-gen-choice: match_function_override``() =
        let expected = @"namespace DU.Tests
{
    using System;
    using System.Collections;

    public partial class None : Maybe<T>
    {
        public override TResult Match<TResult>(Func<TResult> noneFunc, Func<T, TResult> someFunc) => noneFunc();
    }
}
namespace DU.Tests
{
    using System;
    using System.Collections;

    public partial class Some : Maybe<T>
    {
        public override TResult Match<TResult>(Func<TResult> noneFunc, Func<T, TResult> someFunc) => someFunc(Value);
    }
}"
        test_code_gen_choice match_function_override expected

    [<Test>]
    let ``code-gen-choice: ToString``() =
        let expected = @"namespace DU.Tests
{
    using System;
    using System.Collections;

    public partial class None : Maybe<T>
    {
        public override string ToString() => ""None"";
    }
}
namespace DU.Tests
{
    using System;
    using System.Collections;

    public partial class Some : Maybe<T>
    {
        public override string ToString() => String.Format(""Some {0}"", Value);
    }
}"
        test_code_gen_choice tostring_override expected

    [<Test>]
    let ``code-gen-choice: Equals``() =
        let expected = @"namespace DU.Tests
{
    using System;
    using System.Collections;

    public partial class None : Maybe<T>
    {
        public override bool Equals(object other) => other is None;
    }
}
namespace DU.Tests
{
    using System;
    using System.Collections;

    public partial class Some : Maybe<T>
    {
        public override bool Equals(object other) => other is Some && Value.Equals(((Some)other).Value);
    }
}"
        test_code_gen_choice equals_override expected

    [<Test>]
    let ``code-gen-choice: GetHashCode``() =
        let expected = @"namespace DU.Tests
{
    using System;
    using System.Collections;

    public partial class None : Maybe<T>
    {
        public override int GetHashCode() => GetType().FullName.GetHashCode();
    }
}
namespace DU.Tests
{
    using System;
    using System.Collections;

    public partial class Some : Maybe<T>
    {
        public override int GetHashCode() => GetType().FullName.GetHashCode() ^ (Value?.GetHashCode() ?? ""null"".GetHashCode());
    }
}"
        test_code_gen_choice hashcode_override expected

    [<Test>]
    let ``code-gen: wrapper type``() =
        let expected = sprintf @"namespace DU.Tests
{
    using System;
    using System.Collections;

    public abstract partial class Maybe<T> : IEquatable<Maybe<T>>, IStructuralEquatable
    {
        private static partial class ChoiceTypes
        {
            public partial class None : Maybe<T>
            {
                public override TResult Match<TResult>(Func<TResult> noneFunc, Func<T, TResult> someFunc) => noneFunc();
                public override bool Equals(object other) => other is None;
                public override int GetHashCode() => GetType().FullName.GetHashCode();
                public override string ToString() => ""None"";
            }

            public partial class Some : Maybe<T>
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
                public override bool Equals(object other) => other is Some && Value.Equals(((Some)other).Value);
                public override int GetHashCode() => GetType().FullName.GetHashCode() ^ (Value?.GetHashCode() ?? ""null"".GetHashCode());
                public override string ToString() => String.Format(""Some {0}"", Value);
            }
        }
    }
}"
        test_code_gen to_wrapper_type expected

    let COMPLETE_EXPECTED = sprintf @"namespace DU.Tests
{
    using System;
    using System.Collections;

    public abstract partial class Maybe<T> : IEquatable<Maybe<T>>, IStructuralEquatable
    {
        private Maybe()
        {
        }

        public abstract TResult Match<TResult>(Func<TResult> noneFunc, Func<T, TResult> someFunc);
        public static readonly Maybe<T> None = new ChoiceTypes.None();
        public static Maybe<T> NewSome(T value) => new ChoiceTypes.Some(value);
        private static partial class ChoiceTypes
        {
            public partial class None : Maybe<T>
            {
                public override TResult Match<TResult>(Func<TResult> noneFunc, Func<T, TResult> someFunc) => noneFunc();
                public override bool Equals(object other) => other is None;
                public override int GetHashCode() => GetType().FullName.GetHashCode();
                public override string ToString() => ""None"";
            }

            public partial class Some : Maybe<T>
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
                public override bool Equals(object other) => other is Some && Value.Equals(((Some)other).Value);
                public override int GetHashCode() => GetType().FullName.GetHashCode() ^ (Value?.GetHashCode() ?? ""null"".GetHashCode());
                public override string ToString() => String.Format(""Some {0}"", Value);
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

        text_matches (COMPLETE_EXPECTED, actual)
