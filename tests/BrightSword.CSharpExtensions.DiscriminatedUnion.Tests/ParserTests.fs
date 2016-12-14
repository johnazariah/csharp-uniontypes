namespace BrightSword.CSharpExtensions.DiscriminatedUnion.Tests

open BrightSword.CSharpExtensions.DiscriminatedUnion
open NUnit.Framework
open FParsec

module public ParserTests = 
    let private test p str = 
        match run p str with
        | Success(result, _, posn) -> 
            printfn "Success:%s=>%O" str result; true
        | Failure(err, state, _) -> 
            printfn "Failure:%s[%s]" str err; false
    
    let private AssertIsValid   p s = test p s |> Assert.IsTrue
    let private AssertIsInvalid p s = test p s |> Assert.IsFalse

    let private AssertParsesTo p str expected = 
        match run p str with
        | Success(result, _, posn) -> 
            let result' = result.ToString()
            printfn "Success:%s=>%O" str result; 
            Assert.AreEqual(result', expected)
        | Failure(err, state, _) -> 
            printfn "Failure:%s[%s]" str err;
            Assert.Fail "Parse Failed"

    let private AssertIsValidUnion = AssertIsValid unionType
    let private AssertIsInvalidUnion = AssertIsInvalid unionType
    
    [<Test>]
    let ``case class parses``() = 
        let input = "case class Result<T>;"
        let parser = caseClassMember
        AssertIsValid parser input
    
    [<Test>]
    let ``case object parses``() = 
        let input = "case object Exception;"
        let parser = caseObjectMember
        AssertIsValid parser input
    
    [<Test>]
    let ``type - simple name``() = 
        let input = "String"
        AssertIsValid fullTypeName input

    [<Test>]
    let ``type - name with embedded digits``() = 
        let input = "Int32"
        AssertParsesTo fullTypeName input "Int32"
    
    [<Test>]
    let ``type - name with leading digits``() = 
        let input = "2B"
        AssertIsInvalid fullTypeName input
    
    [<Test>]
    let ``type - name with embedded _``() = 
        let input = "This_Is_Good"
        AssertParsesTo fullTypeName input "This_Is_Good"
        
    [<Test>]
    let ``type - name with leading _``() = 
        let input = "_this_is_also_good"
        AssertParsesTo fullTypeName input "_this_is_also_good"

    [<Test>]
    let ``type - name with trailing ?``() = 
        let input = "int?"
        AssertParsesTo fullTypeName input "int?"
    
    [<Test>]
    let ``type - dotted name``() = 
        let input = "System.String"
        AssertParsesTo fullTypeName input "System.String"
    
    [<Test>]
    let ``type - generic name``() = 
        let input = "List<int>"
        AssertParsesTo fullTypeName input "List<int>"

    [<Test>]
    let ``type - generic name with dotted type argument``() = 
        let input = "List<System.String>"
        AssertParsesTo fullTypeName input "List<System.String>"
    
    [<Test>]
    let ``type - dotted generic name``() = 
        let input = "System.Collections.Generic.List<int>"
        AssertParsesTo fullTypeName input "System.Collections.Generic.List<int>"
        
    [<Test>]
    let ``type - dotted generic name with multiple type arguments``() = 
        let input = "System.Collections.Generic.Dictionary<string, int>"
        AssertParsesTo fullTypeName input "System.Collections.Generic.Dictionary<string, int>"

    [<Test>]
    let ``type - dotted generic name with multiple fully qualified type arguments``() = 
        let input = "System.Collections.Generic.Dictionary<string, System.String>"
        AssertParsesTo fullTypeName input "System.Collections.Generic.Dictionary<string, System.String>"

    [<Test>]
    let ``type - nested generic``() = 
        let input = "Something.Lazy<F.Dictionary<int, int>>"
        AssertParsesTo fullTypeName input "Something.Lazy<F.Dictionary<int, int>>"
    
    [<Test>]
    let ``non-generic union parses``() = 
        // let input = @" union TrafficLight { Red | Amber | Green }";
        let input = @"
union TrafficLight 
{     
    case object Red;
    case object Amber;
    case object Green;
}"
        AssertParsesTo unionType input "union TrafficLight ::= [ Red | Amber | Green ]"
    
    [<Test>]
    let ``invalid non-generic union does not parse``() = 
        let input = @"
union TrafficLight[A] 
{ 
    case object Red;
    case object Amber;
    case object Green;
}"
        AssertIsInvalidUnion input
    
    [<Test>]
    let ``generic hybrid union parses``() = 
        //let input = @"union Maybe<T> { Some<T> | None }";
        let input = @"
union Maybe<T> 
{     
    case class Some<T>; 
    case object None; 
}"
        AssertParsesTo unionType input "union Maybe<T> ::= [ Some of T | None ]"
    
    [<Test>]
    let ``total generic union - one argument per case-class``() = 
        let input = @"
union Either<L, R>
{ 
    case class Left<L>; 
    case class Right<R>; 
}"
        AssertParsesTo unionType input "union Either<L, R> ::= [ Left of L | Right of R ]"    
    
    [<Test>]
    let ``total generic union - cannot have more than one generic argument per case-class``() = 
        let input = @"
union Either<L, R> 
{ 
    case class Left<L, R>; 
    case class Right<R>; 
}"
        AssertIsInvalidUnion input
    
    [<Test>]
    let ``total generic union - generic type enclosing type argument``() = 
        let input = @"
union Either<L, R> 
{ 
    case class Left<List<L>>; 
    case class Right<R>; 
}"
        AssertParsesTo unionType input "union Either<L, R> ::= [ Left of List<L> | Right of R ]"    
    
    [<Test>]
    let ``union generic types - union may contain arguments from only some constituent case classes``() = 
        let input = @"
union Result<T> 
{ 
    case class Result<T>; 
    case class Error<Exception>; 
}"
        AssertParsesTo unionType input "union Result<T> ::= [ Result of T | Error of Exception ]"    
    
    [<Test>]
    let ``fully qualified types can be used as case class arguments``() = 
        let input = @"
union Result<T> 
{ 
    case class Result<T>; 
    case class Error<String.Exception>; 
}"
        AssertParsesTo unionType input "union Result<T> ::= [ Result of T | Error of String.Exception ]"    
    
    [<Test>]
    let ``union generic types - union may contain superfluous arguments``() = 
        let input = @"
union Either<X, L, R> 
{ 
    case class Left<L>; 
    case class Right<R>; 
}"
        AssertParsesTo unionType input "union Either<X, L, R> ::= [ Left of L | Right of R ]"    
    
    [<Test>]
    let ``non generic union may have case class members``() = 
        let input = @"
union Payment 
{ 
    case object Cash; 
    case class CreditCard<CreditCardDetails>; 
    case class Cheque<ChequeDetails>; 
}"
        AssertParsesTo unionType input "union Payment ::= [ Cash | CreditCard of CreditCardDetails | Cheque of ChequeDetails ]"    
