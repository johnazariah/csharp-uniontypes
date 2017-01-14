# csharp-uniontypes

Get started with using Union Types right within your existing C# Visual Studio Projects!

Languages like F#, Scala and Haskell have special types to represent a choice of a finite set of values. These types are variously called 'Union Types', 'Sum Types' or 'Discriminated Unions (DUs)'.

Union Types are very useful constructs because they can help model the domain of a problem more precisely, and can help eliminate entire classes of runtime bugs.

Languages like C# and Java can build these types using inheritance and a class hierarchy, but the boilerplate required to do this is too onerous for widespread use.

This library presents a language extension to C# to specify Discriminated Union types, and a tool which parses these specifications and generates idiomatic C# classes which provide the correct functionality.

The tool is available both as a command-line utility and a Visual Studio 2015 extension [also available from the marketplace](https://marketplace.visualstudio.com/items?itemName=JohnAzariah.CUnionTypes).

Follow the [tutorial](https://johnazariah.github.io/csharp-uniontypes/tutorial.html) for more detailed instructions.

## Build Status

Mono | .NET
---- | ----
[![Mono CI Build Status](https://img.shields.io/travis/johnazariah/csharp-uniontypes/master.svg)](https://travis-ci.org/johnazariah/csharp-uniontypes) | [![.NET Build Status](https://img.shields.io/appveyor/ci/johnazariah/csharp-uniontypes/master.svg)](https://ci.appveyor.com/project/johnazariah/csharp-uniontypes)

## Maintainer(s)

- [@johnazariah](https://github.com/johnazariah)
