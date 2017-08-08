namespace CSharp.AlgebraicTypes.Tests

open System.IO
open System.Text.RegularExpressions

open CSharp.AlgebraicTypes

open NUnit.Framework

module IntegratedTestsUsername =
    let ``username.csunion`` = @"namespace Foo
{
    union UserName { UserName<string> }
}"

    let COMPLETE_EXPECTED = sprintf @"namespace Foo
{
    using System;
    using System.Collections;

    public abstract partial class UserName : IEquatable<UserName>, IStructuralEquatable
    {
        private UserName()
        {
        }

        public abstract TResult Match<TResult>(Func<string, TResult> userNameFunc);
        public static UserName NewUserName(string value) => new ChoiceTypes.UserNameClass(value);
        private static partial class ChoiceTypes
        {
            public partial class UserNameClass : UserName
            {
                public UserNameClass(string value)
                {
                    Value = value;
                }

                private string Value
                {
                    get;
                }

                public override TResult Match<TResult>(Func<string, TResult> userNameFunc) => userNameFunc(Value);
                public override bool Equals(object other) => other is UserNameClass && Value.Equals(((UserNameClass)other).Value);
                public override int GetHashCode() => GetType().FullName.GetHashCode() ^ (Value?.GetHashCode() ?? ""null"".GetHashCode());
                public override string ToString() => String.Format(""UserName {0}"", Value);
            }
        }

        public bool Equals(UserName other) => Equals(other as object);
        public bool Equals(object other, IEqualityComparer comparer) => Equals(other);
        public int GetHashCode(IEqualityComparer comparer) => GetHashCode();
        public static bool operator ==(UserName left, UserName right) => left?.Equals(right) ?? false;
        public static bool operator !=(UserName left, UserName right) => !(left == right);
    }
}"
    let complete_expected_with_pragma = (sprintf @"#pragma warning disable CS0660
#pragma warning disable CS0661
%s" COMPLETE_EXPECTED)

    [<Test>]
    let ``code-gen from text: username``() =
        let actual = generate_code_for_text ``username.csunion``
        UnitTestUtilities.text_matches (complete_expected_with_pragma, actual)
