using System;
using System.Collections;
using System.Collections.Generic;

namespace CSharp.AlgebraicTypes.Reference
{
    public sealed partial class Person<G> : IEquatable<Person<G>>, IStructuralEquatable where G : class, IEquatable<G>, IStructuralEquatable
    {
        public G Sex { get; }
        public string Name { get; }
        public int Age { get; }

        public Person(G sex, string name, int age)
        {
            Sex = sex;
            Name = name;
            Age = age;
        }

        public Person<G> With(G sex = null, string name = null, int? age = null)
            => new Person<G>(sex ?? Sex, name ?? Name, age ?? Age);

        public override bool Equals(object other) => Equals(other as Person<G>);

        public bool Equals(object other, IEqualityComparer comparer) => Equals(other);

        public int GetHashCode(IEqualityComparer comparer) => GetHashCode();

        public bool Equals(Person<G> other)
        {
            if (!Sex.Equals(other?.Sex)) return false;
            if (!string.Equals(Name, other?.Name)) return false;
            if (Age != other?.Age) return false;

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = EqualityComparer<G>.Default.GetHashCode(Sex);
                hashCode = (hashCode * 397) ^ (Name?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ Age;
                return hashCode;
            }
        }

        public static bool operator ==(Person<G> left, Person<G> right) => Equals(left, right);

        public static bool operator !=(Person<G> left, Person<G> right) => !Equals(left, right);
    }
}