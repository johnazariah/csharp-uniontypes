#pragma warning disable CS0660
#pragma warning disable CS0661
namespace CoolMonads
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
        public static Maybe<T> Some(T value) => new ChoiceTypes.Some(value);
        private static partial class ChoiceTypes
        {
            public partial class None : Maybe<T>
            {
                public override TResult Match<TResult>(Func<TResult> noneFunc, Func<T, TResult> someFunc) => noneFunc();
                public override bool Equals(object other) => other is None;
                public override int GetHashCode() => GetType().FullName.GetHashCode();
                public override string ToString() => "None";
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
                public override int GetHashCode() => GetType().FullName.GetHashCode() ^ (Value?.GetHashCode() ?? "null".GetHashCode());
                public override string ToString() => String.Format("Some {0}", Value);
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
                public override string ToString() => "Red";
            }

            public partial class Amber : TrafficLights
            {
                public override TResult Match<TResult>(Func<TResult> redFunc, Func<TResult> amberFunc, Func<TResult> greenFunc) => amberFunc();
                public override bool Equals(object other) => other is Amber;
                public override int GetHashCode() => GetType().FullName.GetHashCode();
                public override string ToString() => "Amber";
            }

            public partial class Green : TrafficLights
            {
                public override TResult Match<TResult>(Func<TResult> redFunc, Func<TResult> amberFunc, Func<TResult> greenFunc) => greenFunc();
                public override bool Equals(object other) => other is Green;
                public override int GetHashCode() => GetType().FullName.GetHashCode();
                public override string ToString() => "Green";
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
                public Red() : base(TrafficLights.Red)
                {
                }

                public override TResult Match<TResult>(Func<TResult> redFunc, Func<TResult> amberFunc) => redFunc();
                public override bool Equals(object other) => other is Red;
                public override int GetHashCode() => GetType().FullName.GetHashCode();
                public override string ToString() => "Red";
            }

            public partial class Amber : TrafficLightsToStopFor
            {
                public Amber() : base(TrafficLights.Amber)
                {
                }

                public override TResult Match<TResult>(Func<TResult> redFunc, Func<TResult> amberFunc) => amberFunc();
                public override bool Equals(object other) => other is Amber;
                public override int GetHashCode() => GetType().FullName.GetHashCode();
                public override string ToString() => "Amber";
            }
        }

        public bool Equals(TrafficLightsToStopFor other) => Equals(other as object);
        public bool Equals(object other, IEqualityComparer comparer) => Equals(other);
        public int GetHashCode(IEqualityComparer comparer) => GetHashCode();
        public static bool operator ==(TrafficLightsToStopFor left, TrafficLightsToStopFor right) => left?.Equals(right) ?? false;
        public static bool operator !=(TrafficLightsToStopFor left, TrafficLightsToStopFor right) => !(left == right);
        public static explicit operator TrafficLights(TrafficLightsToStopFor value) => value._base;
    }

    public abstract partial class SingleValue<T> : IEquatable<SingleValue<T>>, IStructuralEquatable
    {
        private readonly Maybe<T> _base;
        private SingleValue(Maybe<T> value)
        {
            _base = value;
        }

        public abstract TResult Match<TResult>(Func<T, TResult> someFunc);
        public static SingleValue<T> Some(T value) => new ChoiceTypes.Some(value);
        private static partial class ChoiceTypes
        {
            public partial class Some : SingleValue<T>
            {
                public Some(T value) : base(Maybe<T>.Some(value))
                {
                    Value = value;
                }

                private T Value
                {
                    get;
                }

                public override TResult Match<TResult>(Func<T, TResult> someFunc) => someFunc(Value);
                public override bool Equals(object other) => other is Some && Value.Equals(((Some)other).Value);
                public override int GetHashCode() => GetType().FullName.GetHashCode() ^ (Value?.GetHashCode() ?? "null".GetHashCode());
                public override string ToString() => String.Format("Some {0}", Value);
            }
        }

        public bool Equals(SingleValue<T> other) => Equals(other as object);
        public bool Equals(object other, IEqualityComparer comparer) => Equals(other);
        public int GetHashCode(IEqualityComparer comparer) => GetHashCode();
        public static bool operator ==(SingleValue<T> left, SingleValue<T> right) => left?.Equals(right) ?? false;
        public static bool operator !=(SingleValue<T> left, SingleValue<T> right) => !(left == right);
        public static explicit operator Maybe<T>(SingleValue<T> value) => value._base;
    }
}