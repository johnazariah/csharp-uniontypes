using System;
using System.Collections;

#pragma warning disable 660,661

// ReSharper disable MemberHidesStaticFromOuterClass

namespace BrightSword.CSharpExtensions.Reference
{
    public abstract class Maybe<T> : IEquatable<Maybe<T>>,
        IStructuralEquatable
    {
        #region Private Constructor

        private Maybe()
        {
        }

        #endregion

        public abstract TResult Match<TResult>(Func<TResult> noneFunc, Func<T, TResult> someFunc);

        private static class ChoiceTypes
        {
            public class None : Maybe<T>
            {
                public override TResult Match<TResult>(Func<TResult> noneFunc, Func<T, TResult> someFunc)
                    => noneFunc();

                public override bool Equals(object other) => other is None;

                public override int GetHashCode() => GetType()
                    .FullName.GetHashCode();

                public override string ToString() => "None";
            }

            public class Some : Maybe<T>
            {
                public Some(T value)
                {
                    Value = value;
                }

                private T Value { get; }

                public override TResult Match<TResult>(Func<TResult> noneFunc, Func<T, TResult> someFunc)
                    => someFunc(Value);

                public override bool Equals(object other)
                    => other is Some && Value.Equals(((Some) other).Value);

                public override int GetHashCode() => GetType()
                    .FullName.GetHashCode() ^ Value.GetHashCode();

                public override string ToString() => string.Format($"Some {Value}");
            }
        }

        #region Equality Semantics

        public bool Equals(Maybe<T> other) => Equals(other as object);

        public bool Equals(object other, IEqualityComparer comparer)
            => Equals(other);

        public int GetHashCode(IEqualityComparer comparer) => GetHashCode();

        public static bool operator ==(Maybe<T> left, Maybe<T> right) => left?.Equals(right) ?? false;

        public static bool operator !=(Maybe<T> left, Maybe<T> right) => !(left == right);

        #endregion

        #region Access Members

        public static readonly Maybe<T> None = new ChoiceTypes.None();
        public static Maybe<T> NewSome(T value) => new ChoiceTypes.Some(value);

        #endregion
    }
}