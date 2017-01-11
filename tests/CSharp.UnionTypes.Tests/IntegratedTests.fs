namespace CSharp.UnionTypes.Tests

open System.IO
open System.Text.RegularExpressions

open CSharp.UnionTypes

open NUnit.Framework

module IntegratedTests =

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
        test_codegen Maybe_T to_wrapper_type expected

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

    public abstract partial class TrafficLights : IEquatable<TrafficLights>, IStructuralEquatable
    {
        private TrafficLights()
        {
        }

        public abstract TResult Match<TResult>(Func<TResult> redFunc, Func<TResult> amberFunc, Func<TResult> greenFunc);
        public static readonly TrafficLights Red = new ChoiceTypes.Red();
        public static readonly TrafficLights Amber = new ChoiceTypes.Amber();
        public static readonly TrafficLights Green = new ChoiceTypes.Green();
        private static partial class ChoiceTypes
        {
            public partial class Red : TrafficLights
            {
                public override TResult Match<TResult>(Func<TResult> redFunc, Func<TResult> amberFunc, Func<TResult> greenFunc) => redFunc();
                public override bool Equals(object other) => other is Red;
                public override int GetHashCode() => GetType().FullName.GetHashCode();
                public override string ToString() => ""Red"";
            }

            public partial class Amber : TrafficLights
            {
                public override TResult Match<TResult>(Func<TResult> redFunc, Func<TResult> amberFunc, Func<TResult> greenFunc) => amberFunc();
                public override bool Equals(object other) => other is Amber;
                public override int GetHashCode() => GetType().FullName.GetHashCode();
                public override string ToString() => ""Amber"";
            }

            public partial class Green : TrafficLights
            {
                public override TResult Match<TResult>(Func<TResult> redFunc, Func<TResult> amberFunc, Func<TResult> greenFunc) => greenFunc();
                public override bool Equals(object other) => other is Green;
                public override int GetHashCode() => GetType().FullName.GetHashCode();
                public override string ToString() => ""Green"";
            }
        }

        public bool Equals(TrafficLights other) => Equals(other as object);
        public bool Equals(object other, IEqualityComparer comparer) => Equals(other);
        public int GetHashCode(IEqualityComparer comparer) => GetHashCode();
        public static bool operator ==(TrafficLights left, TrafficLights right) => left?.Equals(right) ?? false;
        public static bool operator !=(TrafficLights left, TrafficLights right) => !(left == right);
    }

    public abstract partial class TrafficLightsToStopFor : IEquatable<TrafficLightsToStopFor>, IStructuralEquatable
    {
        private readonly TrafficLights _base;
        private TrafficLightsToStopFor(TrafficLights value)
        {
            _base = value;
        }

        public abstract TResult Match<TResult>(Func<TResult> redFunc, Func<TResult> amberFunc);
        public static readonly TrafficLightsToStopFor Red = new ChoiceTypes.Red();
        public static readonly TrafficLightsToStopFor Amber = new ChoiceTypes.Amber();
        private static partial class ChoiceTypes
        {
            public partial class Red : TrafficLightsToStopFor
            {
                public Red() : base(TrafficLights.Red) { }
                public override TResult Match<TResult>(Func<TResult> redFunc, Func<TResult> amberFunc) => redFunc();
                public override bool Equals(object other) => other is Red;
                public override int GetHashCode() => GetType().FullName.GetHashCode();
                public override string ToString() => ""Red"";
            }

            public partial class Amber : TrafficLightsToStopFor
            {
                public Amber() : base(TrafficLights.Amber) { }
                public override TResult Match<TResult>(Func<TResult> redFunc, Func<TResult> amberFunc) => amberFunc();
                public override bool Equals(object other) => other is Amber;
                public override int GetHashCode() => GetType().FullName.GetHashCode();
                public override string ToString() => ""Amber"";
            }
        }

        public bool Equals(TrafficLightsToStopFor other) => Equals(other as object);
        public bool Equals(object other, IEqualityComparer comparer) => Equals(other);
        public int GetHashCode(IEqualityComparer comparer) => GetHashCode();
        public static bool operator ==(TrafficLightsToStopFor left, TrafficLightsToStopFor right) => left?.Equals(right) ?? false;
        public static bool operator !=(TrafficLightsToStopFor left, TrafficLightsToStopFor right) => !(left == right);
        public static explicit operator TrafficLights(TrafficLightsToStopFor value) => value._base;
    }
}"

    [<Test>]
    let ``code-gen: complete``() =
        let actual =
            [ Maybe_T; TrafficLights; TrafficLightsToStopFor ]
            |> List.map (to_class_declaration)
            |> classes_to_code

        text_matches (COMPLETE_EXPECTED, actual)

    let csunion = @"
namespace DU.Tests
{
    union Maybe<T> { None | Some<T> }
    union TrafficLights { Red | Amber | Green }
	union TrafficLightsToStopFor constrains TrafficLights { Red | Amber }
}"
    let complete_expected_with_pragma = (sprintf @"#pragma warning disable CS0660
#pragma warning disable CS0661
%s" COMPLETE_EXPECTED)

    [<Test>]
    let ``code-gen from text: maybe``() =
        let actual = generate_code_for_text csunion
        UnitTestUtilities.text_matches (complete_expected_with_pragma, actual)

    [<Test>]
    let ``code-gen from file: maybe``() =
        let input_file = FileInfo("maybe.csunion")
        Assert.IsTrue <| File.Exists (input_file.FullName)

        let input = File.ReadAllText input_file.FullName
        UnitTestUtilities.text_matches (csunion, input)

        let output_file = Path.Combine(input_file.Directory.FullName, "maybe.g.cs") |> FileInfo
        Assert.IsTrue <| Directory.Exists output_file.Directory.FullName

        File.Delete output_file.FullName
        Assert.IsFalse <| File.Exists (output_file.FullName)

        generate_code_for_csunion_file (Some input_file.FullName, Some output_file.FullName) |> ignore

        Assert.IsTrue <| File.Exists (output_file.FullName)

        let actual = File.ReadAllText output_file.FullName
        UnitTestUtilities.text_matches (complete_expected_with_pragma, actual)
