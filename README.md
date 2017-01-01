# csharp-uniontypes

## Algebraic Data Types
[Algebraic Data Types](https://en.wikipedia.org/wiki/Algebraic_data_type) are composite data types - types that are made up of other types.

### Product Types
In C#, we have only one way of combining types - creating "named tuples" (structs and classes with named properties) and "anonymous tuples" (instances of `Tuple<>`). 

In type algebraic terms, these are called _Product Types_ because the number of valid values of the composite type is the product of the number of valid values of the property types). 
For example, the following struct has 512 possible values because the constituent components have 256 and 2 possible values respectively
```
struct F
{
    char CharacterValue { get; } // 256 possible values
    bool BooleanFlag { get; } // 2 possible values
    ...
} // 256 * 2 = 512 possible values
``` 

Such types are commonly used as encapsulation mechanisms, and for keeping related items together.

### Sum Types
However, languages like F# and Scala have another way of combining types which proves to be very useful in Domain Design. They can be used to specify states very precisely and help make [illegal states unrepresentable](http://www.slideshare.net/ScottWlaschin/domain-driven-design-with-the-f-type-system-functional-londoners-2014).

These types are variously known as _Choice Types_, _Discriminated Union Types_ or _Sum Types_. The valid values of a such a composite type is the _sum_ of the constituent types.

C# programmers can think of these types as "Enums on Steroids", because they represent a choice between values of disparate types.

Consider a domain requirement that stipulates that a payment is recorded as exactly one of the following:

* Cash, where we capture a fixed precision number and an associated currency
* Cheque, where we capture information pertinent to the cheque
* Credit Card, where we capture information pertinent to the credit card

In other words, we require to model a _choice_ between disparate things. Without a formal composite type to model choices, we are generally left to rolling our own mechanisms involving enums and structs. 

However, in F#, we may succintly represent a valid payment as follows:

```
type Payment = 
| Cash of Amount // the currency and amount paid
| Cheque of ChequeDetails // the amount, bank id, cheque number, date ...
| CreditCard of CardDetails // the amount, credit-card type, credit-card expiry date ... 
```

This is far more precise than a record which may introduce illegal states where more than one payment method could be set.

Indeed, if one was willing to include a F# project in their solution and express the domain model in F#, they could simply use the F# types in C# without any further work. 

However, if the domain design is already in C#, and there is resistence to introduce F# into the codebase, this project allows developers to define such _sum types_ in C#, and get the benefits of precise modelling - albeit without the elegant expressivity of F# - without necessarily having to switch languages.

## Build Status

Mono | .NET
---- | ----
[![Mono CI Build Status](https://img.shields.io/travis/johnazariah/csharp-sumtypes/master.svg)](https://travis-ci.org/johnazariah/csharp-sumtypes) | [![.NET Build Status](https://img.shields.io/appveyor/ci/johnazariah/csharp-sumtypes/master.svg)](https://ci.appveyor.com/project/johnazariah/csharp-sumtypes)

## Maintainer(s)

- [@johnazariah](https://github.com/johnazariah)
