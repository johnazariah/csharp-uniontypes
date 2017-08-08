#pragma warning disable CS1718

using System;
using System.Collections.Generic;
using NUnit.Framework;
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable EqualExpressionComparison
// ReSharper disable SuspiciousTypeConversion.Global

namespace BrightSword.CSharpExtensions.Reference
{
    internal class MaybeTests
    {
        [Test]
        public void None_equals_None()
        {
            // disable CS1718
            Assert.True(Maybe<int>.None == Maybe<int>.None);
            Assert.False(Maybe<int>.None != Maybe<int>.None);
            Assert.True(Maybe<int>.None.Equals(Maybe<int>.None));
        }

        [Test]
        public void Some_equals_Some()
        {
            Assert.True(Maybe<int>.Some(10).Equals(Maybe<int>.Some(10)));
            Assert.True(Maybe<int>.Some(10) == Maybe<int>.Some(10));
            Assert.False(Maybe<int>.Some(10) != Maybe<int>.Some(10));
        }

        [Test]
        public void Some_null_Hashes_safely()
        {
            Assert.DoesNotThrow(() => Maybe<string>.Some(null).GetHashCode());

            // should not compile
            //Assert.DoesNotThrow(() => Maybe<int>.Some(null).GetHashCode());
        }

        [Test]
        public void None_hashes_properly()
        {
            var set = new HashSet<Maybe<int>>();
            Assert.AreEqual(0, set.Count);
            set.Add(Maybe<int>.None);
            Assert.AreEqual(1, set.Count);
            set.Add(Maybe<int>.None);
            Assert.AreEqual(1, set.Count);
            set.Add(Maybe<int>.None);
            Assert.AreEqual(1, set.Count);
        }

        [Test]
        public void Some_hashes_properly()
        {
            var set = new HashSet<Maybe<int>>();
            Assert.AreEqual(0, set.Count);
            set.Add(Maybe<int>.Some(10));
            Assert.AreEqual(1, set.Count);
            set.Add(Maybe<int>.Some(10));
            Assert.AreEqual(1, set.Count);
            set.Add(Maybe<int>.Some(20));
            Assert.AreEqual(2, set.Count);
        }

        [Test]
        public void Some_does_not_equal_OtherGenericType()
        {
            Assert.False(Maybe<int>.Some(10)
                                   .Equals(Foo<int>.NewSome(10)));
        }

        [Test]
        public void Some_does_not_equal_null()
        {
            Assert.False(Maybe<int>.Some(10)
                                   .Equals(null));
        }

        [Test]
        public void Some_ToString_works()
        {
            Assert.AreEqual("Some 10", Maybe<int>.Some(10).ToString());
        }

        [Test]
        public void None_ToString_works()
        {
            Assert.AreEqual("None", Maybe<int>.None.ToString());
        }

        private class Foo<T> : IEquatable<Foo<T>>
        {
            private Foo(T value)
            {
                Value = value;
            }

            private T Value { get; }

            public bool Equals(Foo<T> other) => (other != null) && Value.Equals(other.Value);

            public override bool Equals(object other) => Equals(other as Foo<T>);

            public override int GetHashCode() => EqualityComparer<T>.Default.GetHashCode(Value);

            public static bool operator ==(Foo<T> left, Foo<T> right) => Equals(left, right);

            public static bool operator !=(Foo<T> left, Foo<T> right) => !Equals(left, right);

            public static Foo<T> NewSome(T value) => new Foo<T>(value);
        }
    }
}