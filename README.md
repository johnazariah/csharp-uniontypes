# csharp-algebraictypes

Languages like F#, Scala and Haskell have special types to represent a choice of a finite set of values. These types are variously called 'Union Types', 'Sum Types' or 'Discriminated Unions (DUs)'.

Union Types are a powerful way of representing choices. They enforce value semantics and can represent choices between other Record and Union types. They are very useful constructs because they can help model the domain of a problem more precisely, and can help eliminate entire classes of runtime bugs.

Modern C# provides record types, which implicitly implement value semantics; and has suport for pattern matching - both of which make implementation of Union Types possible, if tedious.

This library relieves us of the tedium of building out boilerplate code for Union Types. Instead, one is able to define Union Types in a DSL with syntax that is familiar to C# users, and have the source-generator based library generate the necessary code to support pattern matching and other idiomatic C# features.

The objects generated are extensible so additional methods can be added to them allowing these Union Types to be used in a rich domain model.

Follow the [tutorial](https://johnazariah.github.io/csharp-algebraictypes/tutorial.html) for more detailed instructions.

## Build Status

 .NET
 ----
[![.NET Build Status](https://img.shields.io/appveyor/ci/johnazariah/csharp-uniontypes/master.svg)](https://ci.appveyor.com/project/johnazariah/csharp-uniontypes)

## Maintainer(s)

- [@johnazariah](https://github.com/johnazariah)
