namespace CSharp.UnionTypes.Tests

open CSharp.UnionTypes
open NUnit.Framework

module UnionMemberClassTests =

    [<Test>]
    let ``code-gen-choice: ctor singleton | non-constraining``() =
        let expected = @"namespace DU.Tests
{
    using System;
    using System.Collections;

    public partial class RedClass : TrafficLights
    {
    }
}
namespace DU.Tests
{
    using System;
    using System.Collections;

    public partial class AmberClass : TrafficLights
    {
    }
}
namespace DU.Tests
{
    using System;
    using System.Collections;

    public partial class GreenClass : TrafficLights
    {
    }
}"
        test_codegen_choice TrafficLights ctor expected

    [<Test>]
    let ``code-gen-choice: ctor singleton | constraining``() =
        let expected = @"namespace DU.Tests
{
    using System;
    using System.Collections;

    public partial class RedClass : TrafficLightsToStopFor
    {
        public RedClass() : base(TrafficLights.Red)
        {
        }
    }
}
namespace DU.Tests
{
    using System;
    using System.Collections;

    public partial class AmberClass : TrafficLightsToStopFor
    {
        public AmberClass() : base(TrafficLights.Amber)
        {
        }
    }
}"
        test_codegen_choice TrafficLightsToStopFor ctor expected


    [<Test>]
    let ``code-gen-choice: ctor value | non-constraining``() =
        let expected = @"namespace DU.Tests
{
    using System;
    using System.Collections;

    public partial class NoneClass : Maybe<T>
    {
    }
}
namespace DU.Tests
{
    using System;
    using System.Collections;

    public partial class SomeClass : Maybe<T>
    {
        public SomeClass(T value)
        {
            Value = value;
        }
    }
}"
        test_codegen_choice Maybe_T ctor expected

    [<Test>]
    let ``code-gen-choice: ctor value | constraining``() =
        let expected = @"namespace DU.Tests
{
    using System;
    using System.Collections;

    public partial class SomeClass : SingleValue<T>
    {
        public SomeClass(T value) : base(Maybe<T>.NewSome(value))
        {
            Value = value;
        }
    }
}"
        test_codegen_choice SingleValue_T ctor expected

    [<Test>]
    let ``code-gen-choice: match_function_override``() =
        let expected = @"namespace DU.Tests
{
    using System;
    using System.Collections;

    public partial class NoneClass : Maybe<T>
    {
        public override TResult Match<TResult>(Func<TResult> noneFunc, Func<T, TResult> someFunc) => noneFunc();
    }
}
namespace DU.Tests
{
    using System;
    using System.Collections;

    public partial class SomeClass : Maybe<T>
    {
        public override TResult Match<TResult>(Func<TResult> noneFunc, Func<T, TResult> someFunc) => someFunc(Value);
    }
}"
        test_codegen_choice Maybe_T match_function_override expected

    [<Test>]
    let ``code-gen-choice: ToString``() =
        let expected = @"namespace DU.Tests
{
    using System;
    using System.Collections;

    public partial class NoneClass : Maybe<T>
    {
        public override string ToString() => ""None"";
    }
}
namespace DU.Tests
{
    using System;
    using System.Collections;

    public partial class SomeClass : Maybe<T>
    {
        public override string ToString() => String.Format(""Some {0}"", Value);
    }
}"
        test_codegen_choice Maybe_T tostring_override expected

    [<Test>]
    let ``code-gen-choice: Equals``() =
        let expected = @"namespace DU.Tests
{
    using System;
    using System.Collections;

    public partial class NoneClass : Maybe<T>
    {
        public override bool Equals(object other) => other is NoneClass;
    }
}
namespace DU.Tests
{
    using System;
    using System.Collections;

    public partial class SomeClass : Maybe<T>
    {
        public override bool Equals(object other) => other is SomeClass && Value.Equals(((SomeClass)other).Value);
    }
}"
        test_codegen_choice Maybe_T equals_override expected

    [<Test>]
    let ``code-gen-choice: GetHashCode``() =
        let expected = @"namespace DU.Tests
{
    using System;
    using System.Collections;

    public partial class NoneClass : Maybe<T>
    {
        public override int GetHashCode() => GetType().FullName.GetHashCode();
    }
}
namespace DU.Tests
{
    using System;
    using System.Collections;

    public partial class SomeClass : Maybe<T>
    {
        public override int GetHashCode() => GetType().FullName.GetHashCode() ^ (Value?.GetHashCode() ?? ""null"".GetHashCode());
    }
}"
        test_codegen_choice Maybe_T hashcode_override expected

