using System;
using System.Collections;
using NUnit.Framework;

namespace CSharp.AlgebraicTypes.Reference
{
    internal class PersonTests
    {
        private class Gender : IEquatable<Gender>, IStructuralEquatable
        {
            private string Tag { get; }

            public Gender(string tag)
            {
                Tag = tag;
            }

            public bool Equals(Gender other)
            {
                return string.Equals(this.Tag, other?.Tag);
            }

            public override bool Equals(object other) => Equals(other as Gender);

            public override int GetHashCode()
            {
                return Tag?.GetHashCode() ?? 0;
            }

            public static bool operator ==(Gender left, Gender right) => Equals(left, right);

            public static bool operator !=(Gender left, Gender right) => !Equals(left, right);

            public bool Equals(object other, IEqualityComparer comparer) => Equals(other as Gender);

            public int GetHashCode(IEqualityComparer comparer) => GetHashCode();
        }

        [Test]
        public void Person_Equals_Person()
        {
            var left = new Person<Gender>(new Gender("Male"), "John", 42);
            var right = new Person<Gender>(new Gender("Male"), "John", 42);

            Assert.AreEqual(left, right);
        }

        [Test]
        public void Person_Single_With_Generates_Equatable_Object()
        {
            var orig = new Person<Gender>(new Gender("Male"), "Jim", 42);
            var left = orig.With(name: "John");
            var right = new Person<Gender>(new Gender("Male"), "John", 42);

            Assert.AreNotEqual(orig, left);
            Assert.AreEqual(left, right);
        }

        [Test]
        public void Person_Multiple_With_Generates_Equatable_Object()
        {
            var orig = new Person<Gender>(new Gender("Male"), "Jim", 40);
            var left = orig.With(name: "John").With(age: 42);

            var right = new Person<Gender>(new Gender("Male"), "John", 42);

            Assert.AreNotEqual(orig, left);
            Assert.AreEqual(left, right);
        }

    }
}