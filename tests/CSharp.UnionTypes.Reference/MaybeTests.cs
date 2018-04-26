using System;
using System.Collections.Generic;
using Xunit;

namespace tests.reference.maybe
{
    public class MaybeTests
    {
        [Fact]
        public void None_equals_None()
        {
            // disable CS1718
            Assert.True(Maybe<int>.None == Maybe<int>.None);
            Assert.False(Maybe<int>.None != Maybe<int>.None);
            Assert.True(Maybe<int>.None.Equals(Maybe<int>.None));
        }

        [Fact]
        public void Some_equals_Some()
        {
            Assert.True(Maybe<int>.Some(10).Equals(Maybe<int>.Some(10)));
            Assert.True(Maybe<int>.Some(10) == Maybe<int>.Some(10));
            Assert.False(Maybe<int>.Some(10) != Maybe<int>.Some(10));
        }

        [Fact]
        public void Some_null_Hashes_safely()
        {
            var ex = Record.Exception(() => Maybe<string>.Some(null).GetHashCode());
            Assert.Null(ex);

            // should not compile
            //Assert.DoesNotThrow(() => Maybe<int>.Some(null).GetHashCode());
        }

        [Fact]
        public void None_hashes_properly()
        {
            var set = new HashSet<Maybe<int>>();
            Assert.Empty(set);
            set.Add(Maybe<int>.None);
            Assert.Single(set);
            set.Add(Maybe<int>.None);
            Assert.Single(set);
            set.Add(Maybe<int>.None);
            Assert.Single(set);
        }

        [Fact]
        public void Some_hashes_properly()
        {
            var set = new HashSet<Maybe<int>>();
            Assert.Empty(set);
            set.Add(Maybe<int>.Some(10));
            Assert.Single(set);
            set.Add(Maybe<int>.Some(10));
            Assert.Single(set);
            set.Add(Maybe<int>.Some(20));
            Assert.Equal(2, set.Count);
        }

        [Fact]
        public void Some_does_not_equal_OtherGenericType()
        {
            Assert.False(Maybe<int>.Some(10)
                                   .Equals(Foo<int>.NewSome(10)));
        }

        [Fact]
        public void Some_does_not_equal_null()
        {
            Assert.False(Maybe<int>.Some(10)
                                   .Equals(null));
        }

        [Fact]
        public void Some_ToString_works()
        {
            Assert.Equal("Some 10", Maybe<int>.Some(10).ToString());
        }

        [Fact]
        public void None_ToString_works()
        {
            Assert.Equal("None", Maybe<int>.None.ToString());
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
