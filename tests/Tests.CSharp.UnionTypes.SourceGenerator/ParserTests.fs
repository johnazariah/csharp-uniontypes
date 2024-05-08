module CSharp.UnionTypes.SourceGenerator.ParserTests

open CSharp.UnionTypes
open Xunit
open FParsec

let private test p str =
    match run p str with
    | Success(result, _, posn) ->
        printfn "Success:%s=>%O" str result
        true
    | Failure(err, state, _) ->
        printfn "Failure:%s[%s]" str err
        false

let private AssertIsValid p s = test p s |> Assert.True
let private AssertIsInvalid p s = test p s |> Assert.False

let private AssertParsesTo p str expected =
    match run p str with
    | Success(result, _, posn) ->
        let result' = result.ToString()
        printfn "Success:%s=>%O" str result
        Assert.Equal(result', expected)
    | Failure(err, state, _) ->
        printfn "Failure:%s[%s]" str err
        Assert.Fail "Parse Failed"

let private AssertIsValidUnion = AssertIsValid unionType
let private AssertIsInvalidUnion = AssertIsInvalid unionType

[<Fact>]
let ``parser: case class parses``() =
    let input = "Result<T>"
    let parser = caseMember
    AssertParsesTo parser input "Result of T"

[<Fact>]
let ``parser: case object parses``() =
    let input = "Exception;"
    let parser = caseMember
    AssertParsesTo parser input "Exception"

[<Fact>]
let ``parser: type - simple name``() =
    let input = "String"
    AssertIsValid fullTypeName input

[<Fact>]
let ``parser: type - name with embedded digits``() =
    let input = "Int32"
    AssertParsesTo fullTypeName input "Int32"

[<Fact>]
let ``parser: type - name with leading digits``() =
    let input = "2B"
    AssertIsInvalid fullTypeName input

[<Fact>]
let ``parser: type - name with embedded _``() =
    let input = "This_Is_Good"
    AssertParsesTo fullTypeName input "This_Is_Good"

[<Fact>]
let ``parser: type - name with leading _``() =
    let input = "_this_is_also_good"
    AssertParsesTo fullTypeName input "_this_is_also_good"

[<Fact>]
let ``parser: type - name with trailing ?``() =
    let input = "int?"
    AssertParsesTo fullTypeName input "int?"

[<Fact>]
let ``parser: type - dotted name``() =
    let input = "System.String"
    AssertParsesTo fullTypeName input "System.String"

[<Fact>]
let ``parser: type - generic name``() =
    let input = "List<int>"
    AssertParsesTo fullTypeName input "List<int>"

[<Fact>]
let ``parser: type - generic name with dotted type argument``() =
    let input = "List<System.String>"
    AssertParsesTo fullTypeName input "List<System.String>"

[<Fact>]
let ``parser: type - dotted generic name``() =
    let input = "System.Collections.Generic.List<int>"
    AssertParsesTo fullTypeName input "System.Collections.Generic.List<int>"

[<Fact>]
let ``parser: type - dotted generic name with multiple type arguments``() =
    let input = "System.Collections.Generic.Dictionary<string, int>"
    AssertParsesTo fullTypeName input "System.Collections.Generic.Dictionary<string, int>"

[<Fact>]
let ``parser: type - dotted generic name with multiple fully qualified type arguments``() =
    let input = "System.Collections.Generic.Dictionary<string, System.String>"
    AssertParsesTo fullTypeName input "System.Collections.Generic.Dictionary<string, System.String>"

[<Fact>]
let ``parser: type - nested generic``() =
    let input = "Something.Lazy<F.Dictionary<int, int>>"
    AssertParsesTo fullTypeName input "Something.Lazy<F.Dictionary<int, int>>"

[<Fact>]
let ``parser: union - non-generic union parses``() =
    let input = @" union TrafficLight { Red | Amber | Green }";
    AssertParsesTo unionType input "union TrafficLight ::= [ Red | Amber | Green ]"

[<Fact>]
let ``parser: union - invalid non-generic union does not parse``() =
    let input = @"
union TrafficLight[A]
{
    Red
    | Amber
    | Green
}"
    AssertIsInvalidUnion input

[<Fact>]
let ``parser: union - generic hybrid union parses``() =
    let input = @"union Maybe<T> { Some<T> | None }";
    AssertParsesTo unionType input "union Maybe<T> ::= [ Some of T | None ]"

[<Fact>]
let ``parser: union - total generic union - one argument per case-class``() =
    let input = @"
union Either<L, R>
{
    Left<L> | Right<R>
}"
    AssertParsesTo unionType input "union Either<L, R> ::= [ Left of L | Right of R ]"

[<Fact>]
let ``parser: union - total generic union - cannot have more than one generic argument per case-class``() =
    let input = @"
union Either<L, R>
{
    Left<L, R> | Right<R>
}"
    AssertIsInvalidUnion input

[<Fact>]
let ``parser: union - total generic union - generic type enclosing type argument``() =
    let input = @"union Either<L, R> { Left<List<L>> | Right<R> }"
    AssertParsesTo unionType input "union Either<L, R> ::= [ Left of List<L> | Right of R ]"

[<Fact>]
let ``parser: union - union generic types - union may contain arguments from only some constituent case classes``() =
    let input = @"
union Result<T>
{
    Result<T> | Error<Exception>
}"
    AssertParsesTo unionType input "union Result<T> ::= [ Result of T | Error of Exception ]"

[<Fact>]
let ``parser: union - fully qualified types can be used as case class arguments``() =
    let input = @"union Result<T> { Result<T> | Error<System.Exception> }"
    AssertParsesTo unionType input "union Result<T> ::= [ Result of T | Error of System.Exception ]"

[<Fact>]
let ``parser: union - union generic types - union may contain superfluous arguments``() =
    let input = @"
union Either<X, L, R>
{
    Left<L> | Right<R>
}"
    AssertParsesTo unionType input "union Either<X, L, R> ::= [ Left of L | Right of R ]"

[<Fact>]
let ``parser: union - non generic union may have case class members``() =
    let input = @"union Payment { Cash | CreditCard<CreditCardDetails> | Cheque<ChequeDetails> }"
    AssertParsesTo unionType input
        "union Payment ::= [ Cash | CreditCard of CreditCardDetails | Cheque of ChequeDetails ]"

[<Fact>]
let ``parser: using - simple case``() =
    let input = @"
using System.Collections.Generic;
"
    AssertParsesTo using input "System.Collections.Generic"

[<Fact>]
let ``parser: namespace - empty``() =
    let input = @"
namespace CoolMonads
{
}
"
    AssertParsesTo ``namespace`` input "namespace CoolMonads{}"

[<Fact>]
let ``parser: namespace - dotted name``() =
    let input = @"
namespace DU.Tests
{
}
"
    AssertParsesTo ``namespace`` input "namespace DU.Tests{}"

[<Fact>]
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
        "namespace CoolMonads{System; Payment}"
    AssertParsesTo ``namespace`` input expected

[<Fact>]
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
        "namespace CoolMonads{System; System.Collections.Generic; Payment; Result<T>}"
    AssertParsesTo ``namespace`` input expected
