CSharp.UnionTypes
========================

## Documentation

### Install
-------

The JohnAz.CSharp.UnionTypes library can be [installed from NuGet](https://www.nuget.org/packages/JohnAz.CSharp.UnionTypes).

  <pre>PM> NuGet\Install-Package JohnAz.CSharp.UnionTypes</pre>

### Example
-------

Unions are defined in `.csunion` files using a little DSL, which are processed by a [Source Generator](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview), which then generates the appropriate objects to implement the  discriminated union in C#.

Consider a `.csunion` file containing the following text:

```c++

namespace Monads
{
  union Maybe<T> { Some<T> | None };
}
```

This signifies that we want a type called `Maybe<T>` which can be _either_ a value of type `Some<T>` or of `None`.

Using value semantics provided by records in C# 10.0 and above, we can then generate the following code which implements the discriminated union.

```csharp
namespace Monads
{
    public abstract partial record Maybe<T>
    {
        private Maybe() { }
        public sealed partial record Some(T Value) : Maybe<T>;
        public sealed partial record None() : Maybe<T>;
    }
}
```

Then, when we wanted to use the `Maybe<T>` type in our code, we could make use of [switch expressions](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/switch-expression) to properly handle the various cases.

```csharp
public static void Main (string[] args)
{
    Maybe<int> m23 = new Maybe<int>.Some(23);

    Console.WriteLine(m23 switch
    {
        Maybe<int>.Some { Value: var v } => $"Some {v}",
        Maybe<int>.None => "None",
        _ => throw new NotImplementedException()
    });
}
```

In the code above, we create a `Maybe<int>` value of type `Maybe<int>.Some` wrapping an `int` value of `23`, and then use a switch expression to print out the value `Some 23`.

To summarize

1. Install the [Nuget Package](https://www.nuget.org/packages/JohnAz.CSharp.UnionTypes)
2. Add a file of type `.csunion` in your project and set its type to `Analyser Additional File`. 
3. Write an expression to define a Union type in the `.csunion` file above.
4. Use the type as if you had written it yourself in C#. The source generator will have generated it for you in the background.
5. Enjoy!

### Contributing and copyright
--------------------------

The project is hosted on [GitHub][gh] where you can [report issues][issues], fork
the project and submit pull requests. If you're adding a new public API, please also
consider adding [samples][content] that can be turned into a documentation, or consider improving the [tutorial](tutorial.md). You might
also want to read the [library design notes][readme] to understand how it works.

The library is available under Public Domain license, which allows modification and
redistribution for both commercial and non-commercial purposes. For more information see the
[License file][license] in the GitHub repository.

  [content]: https://github.com/johnazariah/csharp-uniontypes/tree/master/docs/content
  [gh]:      https://github.com/johnazariah/csharp-uniontypes
  [issues]:  https://github.com/johnazariah/csharp-uniontypes/issues
  [readme]:  https://github.com/johnazariah/csharp-uniontypes/blob/master/README.md
  [license]: https://github.com/johnazariah/csharp-uniontypes/blob/master/LICENSE.txt
