namespace CSharp.AlgebraicTypes.Tests

open CSharp.AlgebraicTypes
open NUnit.Framework

module UnionTypeClassTests =

    [<Test>]
    let ``code-gen: private constructor - non-constraining``() =
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
        test_codegen Maybe_T to_private_ctor expected

    [<Test>]
    let ``code-gen: private constructor - constraining``() =
        let expected = @"namespace DU.Tests
{
    using System;
    using System.Collections;

    public abstract partial class TrafficLightsToStopFor : IEquatable<TrafficLightsToStopFor>, IStructuralEquatable
    {
        private TrafficLightsToStopFor(TrafficLights value)
        {
            _base = value;
        }
    }
}"
        test_codegen TrafficLightsToStopFor to_private_ctor expected

    [<Test>]
    let ``code-gen: base value field - non-constraining``() =
        let expected = @"namespace DU.Tests
{
    using System;
    using System.Collections;

    public abstract partial class Maybe<T> : IEquatable<Maybe<T>>, IStructuralEquatable
    {
    }
}"
        test_codegen Maybe_T to_base_value expected

    [<Test>]
    let ``code-gen: base value field - constraining``() =
        let expected = @"namespace DU.Tests
{
    using System;
    using System.Collections;

    public abstract partial class TrafficLightsToStopFor : IEquatable<TrafficLightsToStopFor>, IStructuralEquatable
    {
        private readonly TrafficLights _base;
    }
}"
        test_codegen TrafficLightsToStopFor to_base_value expected

    [<Test>]
    let ``code-gen: base cast operator - non-constraining``() =
        let expected = @"namespace DU.Tests
{
    using System;
    using System.Collections;

    public abstract partial class TrafficLights : IEquatable<TrafficLights>, IStructuralEquatable
    {
    }
}"
        test_codegen TrafficLights to_base_cast expected

    [<Test>]
    let ``code-gen: base cast operator - singleton | constraining``() =
        let expected = @"namespace DU.Tests
{
    using System;
    using System.Collections;

    public abstract partial class TrafficLightsToStopFor : IEquatable<TrafficLightsToStopFor>, IStructuralEquatable
    {
        public static explicit operator TrafficLights(TrafficLightsToStopFor value) => value._base;
    }
}"
        test_codegen TrafficLightsToStopFor to_base_cast expected

    [<Test>]
    let ``code-gen: base cast operator - value | constraining``() =
        let expected = @"namespace DU.Tests
{
    using System;
    using System.Collections;

    public abstract partial class SingleValue<T> : IEquatable<SingleValue<T>>, IStructuralEquatable
    {
        public static explicit operator Maybe<T>(SingleValue<T> value) => value._base;
    }
}"
        test_codegen SingleValue_T to_base_cast expected

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
        test_codegen Maybe_T to_match_function_abstract expected
    [<Test>]
    let ``code-gen: access members``() =
        let expected = @"namespace DU.Tests
{
    using System;
    using System.Collections;

    public abstract partial class Maybe<T> : IEquatable<Maybe<T>>, IStructuralEquatable
    {
        public static readonly Maybe<T> None = new ChoiceTypes.NoneClass();
        public static Maybe<T> NewSome(T value) => new ChoiceTypes.SomeClass(value);
    }
}"
        test_codegen Maybe_T to_access_members expected

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
        test_codegen Maybe_T to_equatable_equals_method expected

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
        test_codegen Maybe_T to_structural_equality_equals_method expected

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
        test_codegen Maybe_T to_structural_equality_gethashcode_method expected

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
        test_codegen Maybe_T to_eq_operator expected


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
        test_codegen Maybe_T to_neq_operator expected

