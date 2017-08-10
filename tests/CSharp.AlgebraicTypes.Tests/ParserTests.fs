namespace CSharp.AlgebraicTypes.Tests

open CSharp.AlgebraicTypes
open NUnit.Framework
open FParsec

module public ParserTests =
    let private test p str =
        match run p str with
        | Success(result, _, posn) ->
            printfn "Success:%s=>%O" str result
            true
        | Failure(err, state, _) ->
            printfn "Failure:%s[%s]" str err
            false

    let private AssertIsValid p s = test p s |> Assert.IsTrue
    let private AssertIsInvalid p s = test p s |> Assert.IsFalse

    let private AssertParsesTo p str expected =
        match run p str with
        | Success(result, _, posn) ->
            let result' = result.ToString()
            printfn "Success:%s=>%O" str result
            Assert.AreEqual(result', expected)
        | Failure(err, state, _) ->
            printfn "Failure:%s[%s]" str err
            Assert.Fail "Parse Failed"

    let private AssertIsValidUnion = AssertIsValid unionType
    let private AssertIsInvalidUnion = AssertIsInvalid unionType

    [<Test>]
    let ``parser: case class parses``() =
        let input = "Result<T>"
        let parser = unionTypeMember
        AssertParsesTo parser input "Result<T>"

    [<Test>]
    let ``parser: case object parses``() =
        let input = "Exception"
        let parser = unionTypeMember
        AssertParsesTo parser input "Exception"

    [<Test>]
    let ``parser: case class with numbers parses``() =
        let input = "Case1Of2<T>"
        let parser = unionTypeMember
        AssertParsesTo parser input "Case1Of2<T>"

    [<Test>]
    let ``parser: case object with numbers parses``() =
        let input = "Case2Of2"
        let parser = unionTypeMember
        AssertParsesTo parser input "Case2Of2"

    [<Test>]
    let ``parser: type - simple name``() =
        let input = "String"
        AssertIsValid typeReference input

    [<Test>]
    let ``parser: type - name with embedded digits``() =
        let input = "Int32"
        AssertParsesTo typeReference input "Int32"

    [<Test>]
    let ``parser: type - name with leading digits``() =
        let input = "2B"
        AssertIsInvalid typeReference input

    [<Test>]
    let ``parser: type - name with embedded _``() =
        let input = "This_Is_Good"
        AssertParsesTo typeReference input "This_Is_Good"

    [<Test>]
    let ``parser: type - name with leading _``() =
        let input = "_this_is_also_good"
        AssertParsesTo typeReference input "_this_is_also_good"

    [<Test>]
    let ``parser: type - name with trailing ?``() =
        let input = "int?"
        AssertParsesTo typeReference input "int?"

    [<Test>]
    let ``parser: type - dotted name``() =
        let input = "System.String"
        AssertParsesTo typeReference input "System.String"

    [<Test>]
    let ``parser: type - generic name``() =
        let input = "List<int>"
        AssertParsesTo typeReference input "List<int>"

    [<Test>]
    let ``parser: type - generic name with dotted type argument``() =
        let input = "List<System.String>"
        AssertParsesTo typeReference input "List<System.String>"

    [<Test>]
    let ``parser: type - dotted generic name``() =
        let input = "System.Collections.Generic.List<int>"
        AssertParsesTo typeReference input "System.Collections.Generic.List<int>"

    [<Test>]
    let ``parser: type - dotted generic name with multiple type arguments``() =
        let input = "System.Collections.Generic.Dictionary<string, int>"
        AssertParsesTo typeReference input "System.Collections.Generic.Dictionary<string, int>"

    [<Test>]
    let ``parser: type - dotted generic name with multiple fully qualified type arguments``() =
        let input = "System.Collections.Generic.Dictionary<string, System.String>"
        AssertParsesTo typeReference input "System.Collections.Generic.Dictionary<string, System.String>"

    [<Test>]
    let ``parser: type - nested generic``() =
        let input = "Something.Lazy<F.Dictionary<int, int>>"
        AssertParsesTo typeReference input "Something.Lazy<F.Dictionary<int, int>>"

    [<Test>]
    let ``parser: union - non-generic union parses``() =
        let input = @" union TrafficLight { Red | Amber | Green }";
        AssertParsesTo unionType input "union TrafficLight { Red | Amber | Green }"

    [<Test>]
    let ``parser: record - non-generic record parses``() =
        let input = @" record Person { Name : string; Age : int }";
        AssertParsesTo recordType input "record Person { Name : string; Age : int }"

    [<Test>]
    let ``parser: union - invalid non-generic union does not parse``() =
        let input = @"
union TrafficLight[A]
{
    Red
    | Amber
    | Green
}"
        AssertIsInvalidUnion input

    [<Test>]
    let ``parser: union - generic hybrid union parses``() =
        let input = @"union Maybe<T> { Some<T> | None }";
        AssertParsesTo unionType input "union Maybe<T> { Some<T> | None }"

    [<Test>]
    let ``parser: record - generic record parses``() =
        let input = @"record Person<Gender> { Sex : Gender; Name : string }";
        AssertParsesTo recordType input "record Person<Gender> { Sex : Gender; Name : string }"

    [<Test>]
    let ``parser: union - total generic union - one argument per case-class``() =
        let input = @"
union Either<L, R>
{
    Left<L> | Right<R>
}"
        AssertParsesTo unionType input "union Either<L, R> { Left<L> | Right<R> }"

    [<Test>]
    let ``parser: union - total generic union - cannot have more than one generic argument per case-class``() =
        let input = @"
union Either<L, R>
{
    Left<L, R> | Right<R>
}"
        AssertIsInvalidUnion input

    [<Test>]
    let ``parser: union - total generic union - generic type enclosing type argument``() =
        let input = @"union Either<L, R> { Left<List<L>> | Right<R> }"
        AssertParsesTo unionType input "union Either<L, R> { Left<List<L>> | Right<R> }"

    [<Test>]
    let ``parser: union - union generic types - union may contain arguments from only some constituent case classes``() =
        let input = @"
union Result<T>
{
    Result<T> | Error<Exception>
}"
        AssertParsesTo unionType input "union Result<T> { Result<T> | Error<Exception> }"

    [<Test>]
    let ``parser: union - fully qualified types can be used as case class arguments``() =
        let input = @"union Result<T> { Result<T> | Error<System.Exception> }"
        AssertParsesTo unionType input "union Result<T> { Result<T> | Error<System.Exception> }"

    [<Test>]
    let ``parser: union - union generic types - union may contain superfluous arguments``() =
        let input = @"
union Either<X, L, R>
{
    Left<L> | Right<R>
}"
        AssertParsesTo unionType input "union Either<X, L, R> { Left<L> | Right<R> }"

    [<Test>]
    let ``parser: union - non generic union may have case class members``() =
        let input = @"union Payment { Cash | CreditCard<CreditCardDetails> | Cheque<ChequeDetails> }"
        AssertParsesTo unionType input "union Payment { Cash | CreditCard<CreditCardDetails> | Cheque<ChequeDetails> }"

    [<Test>]
    let ``parser: using - simple case``() =
        let input = @"using System.Collections.Generic;"

        AssertParsesTo using input "using System.Collections.Generic;"

    [<Test>]
    let ``parser: namespace - empty``() =
        let input = @"
namespace CoolMonads
{
}
"
        AssertParsesTo ``namespace`` input "namespace CoolMonads {  }"

    [<Test>]
    let ``parser: namespace - dotted name``() =
        let input = @"
namespace DU.Tests
{
}
"
        AssertParsesTo ``namespace`` input @"namespace DU.Tests {  }"

    [<Test>]
    let ``parser: namespace - single using and union``() =
        let input = @"
namespace CoolMonads
{
    using System;

    union Payment
    {
        Cash
        | CreditCard<CreditCardDetails>
        | Cheque<ChequeDetails>
    }
}
"
        let expected =
            "namespace CoolMonads { using System;; union Payment { Cash | CreditCard<CreditCardDetails> | Cheque<ChequeDetails> } }"
        AssertParsesTo ``namespace`` input expected

    [<Test>]
    let ``parser: namespace - multiple using and union``() =
        let input = @"
namespace CoolMonads
{
    using System;
    using System.Collections.Generic;

    union Payment
    {
        Cash
        | CreditCard<CreditCardDetails>
        | Cheque<ChequeDetails>
    }

    union Result<T> { Result<T> | Error<System.Exception> }
}
"
        let expected =
            "namespace CoolMonads { using System;; using System.Collections.Generic;; union Payment { Cash | CreditCard<CreditCardDetails> | Cheque<ChequeDetails> }; union Result<T> { Result<T> | Error<System.Exception> } }"
        AssertParsesTo ``namespace`` input expected

    [<Test>]
    let ``parser: namespace - using, union and record``() =
        let input = @"
namespace CoolMonads
{
    using System;
    using System.Collections.Generic;

    union Payment
    {
        Cash
        | CreditCard<CreditCardDetails>
        | Cheque<ChequeDetails>
    }

    record Person
    {
        Name : string ;
        Age : int
    }
}
"
        let expected =
            "namespace CoolMonads { using System;; using System.Collections.Generic;; union Payment { Cash | CreditCard<CreditCardDetails> | Cheque<ChequeDetails> }; record Person { Name : string; Age : int } }"
        AssertParsesTo ``namespace`` input expected
