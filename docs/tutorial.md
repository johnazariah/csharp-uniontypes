
Union Types for C#
========================

## Summary

This library provides a source-generator based solution to allow modelling of union types within a C# project.

It defines a minimal extension to the C# language, and automatically generate idiomatic C# classes which provide the functionality of union types.

## Structure of a .csunion file

The `.csunion` file should have the following structure:

```
namespace <namespace-name>
{
    using <some-namespace>;

    union <union-name>
    {
        <union-member> | <union-member> | <union-member>;
    }
}
```

### namespace

The outermost element must be a _single_ `namespace` element with a valid (possibly fully-qualified) namespace-name.

The classes generated will be placed into this namespace, and your C# code can simply reference the namespace with a normal `using`.abs

### using

Any number of `using` statements can be present. Each should terminate with a `;` and be on a separate line.

The generated file will include these `using` statements. 

Normally valid `using` statements are supported, but aliasing is not.

This element is supported  so that the generated file can compile without further editing, so typically you will need to specify assemblies containing types referenced by the union type.abs

_The `System` and `System.Collections` namespaces are always included automatically even without explicit specification._

### union

Any number of `union` statements can be present. Each may terminate with a `;` and must start on a separate line.

This element specifies the Discriminated Union type.

The structure of the `union` statement is:

```
    union <union-name>
    {
        <union-member> | <union-member> | <union-member>;
    }
```

* union-name : This will be the name of the Union Type generated. It should be a valid class name, and can specify type arguments if the union type is to be generic. 
_You cannot specify generic type constraints in this file. Create a partial class with the same name and type arguments in another `.cs` file in the project and include the generic type constraints on that._

* union-member : There can be any number of members. Each member represents a choice in the union.
A union members can either be:
    * Singleton : In this case, the union-member has exactly one instance with the same name as the member.
    * Value-Constructor : In this case, the union-member is parametrized by the type specified, and will be an instance of a class with the same name as the member associated with a value of the parametrizing type.

Example:
```
    union Maybe<T> { None | Some<T> }
```
This specifies that `Maybe<int>` is either the value `None`, or `Some` with an associated `int`.

Some illustrative examples are:

#### Enumerations

```
    union TrafficLights { Red | Amber | Green }
```

This discriminated union type is a union of singleton values. _i.e._ The `TrafficLights` type can have a value that is either `Red`, `Amber` or `Green`. These _enum_-like types are very useful in providing closed sets of values, and also with _constrained types_.

#### Value Constructors

```
    union Maybe<T> { None | Some<T> }
```
This discriminated union type represents a choice between the singleton value `None`, and an instance of a class `Some` wrapping a value of type `T`. In this case, the discriminated union type is itself generic and takes `T` as a type argument.

_Note that this discriminated union shows the use of a choice between a singleton and a parametrized value. Such choices are perfectly legal_

```
    union Payment { Cash<Amount> | CreditCard<CreditCardDetails> | Cheque<ChequeDetails> }
```
This discriminated union type is non generic and represents a choice between an instance of `Cash` (parameterized by `Amount`), `CreditCard` (parametrized by `CreditCardDetails`), and `Cheque` (parametrized by `ChequeDetails`).

_Note that in this case, one or more `using` directives including the assembly (or assemblies) containing the definitions of `Amount`, `CreditCardDetails`, and `ChequeDetails` will need to be specified for the generated file to compile._

```
    union Either<L, R> { Left<L> | Right<R> }
```
This discriminated union demonstrates multiple type parameters.

#### Constrained Types
```
    union TrafficLightsToStopFor constrains TrafficLights { Red | Amber }
```
Typically, classes are specified with base functionality, which can be augmented by derived classes. With union types, however, there is often a benefit to defining a type that represents _a subset of_ another type's members.

The `constrains` keyword allows for such a specification.

* **It is illegal to specify a member in a constrained type that does not exist in the type it is constraining.**

## How to code against a union type

Once the specification has been transformed into a C# class, it can be directly used in any C# code.

### Specifying the Choice

Creating an instance of the union type does not involve `new`. Indeed, it is not possible to `new` up a Union Type because it is represented by an abstract class.

Instead, one must use static members provided in the abstract class to construct instances as desired.

#### 'Singleton' choices
For singleton choices, you can simply reference the readonly singleton member as follows:

```
    var none = new Maybe<string>.None();
```

#### 'Value Constructor' choices
For value constructor choices, you will need to provide the value to the constructor member as follows:

```
    var name = new Maybe<string>.Some("John");
```

### Augmenting the partial class

All the generated code is in the form of partial classes, which allows methods to be attached to the Union Type from within the C# project.

For example, we can extend the `Maybe<T>` class with functional typeclasses by providing an associate C# file defining another partial class of it.

```
public partial class Maybe<T>
{
    // Functor
    public Maybe<R> Map<R>(Func<T, R> f) => Match(() => Maybe<R>.None, _ => Maybe<R>.Some(f(_)));

    // Applicative
    public static Maybe<Y> Apply<X, Y>(Maybe<Func<X, Y>> f, Maybe<X> x) => f.Match(() => Maybe<Y>.None, _f => x.Match(() => Maybe<Y>.None, _x => Maybe<Y>.Some(_f(_x))));

    // Foldable
    public R Fold<R>(R z, Func<R, T, R> f) => Match(() => z, _ => f(z, _));

    // Monad
    public static Maybe<T> Unit(T value) => Some(value);
    public Maybe<R> Bind<R>(Func<T, Maybe<R>> f) => Match(() => Maybe<R>.None, f);

    public T GetOrElse(T defaultValue) => Fold(defaultValue, (_, v) => v);
}

// LINQ extensions
public static class MaybeLinqExtensions
{
    public static Maybe<T> Lift<T>(this T value) => Maybe<T>.Unit(value);

    public static Maybe<TR> Select<T, TR>(this Maybe<T> m, Func<T, TR> f) => m.Map(f);

    public static Maybe<TR> SelectMany<T, TR>(this Maybe<T> m, Func<T, Maybe<TR>> f) => m.Bind(f);

    public static Maybe<TV> SelectMany<T, TU, TV>(this Maybe<T> m, Func<T, Maybe<TU>> f, Func<T, TU, TV> s)
        => m.SelectMany(x => f(x).SelectMany(y => (Maybe<TV>.Unit(s(x, y)))));
}
``` 

which allows us to use the class in fully augmented form in our code as follows:

```
var lenOpt = from s in Maybe<string>.Some("Hello, World")
                select s.Length;
Console.WriteLine($"The length of the given string is {lenOpt.GetOrElse(0)}");
```

### Value Semantics

The union type implementation uses classes, which means that simply comparing two instances for equality within C# code will result in reference comparision.

However, we want to make sure that `Maybe<String>.Some("A")` will always be equal to `Maybe<String>.Some("A")` - regardless of whether they were instantiated separately or are both the same object.

The generated code for union types implement `IEquatable<T>` and `IStructuralEquatable` for each union, and override the appropriate types to provide value semantics for equality.

```
[Test]
public void Some_equals_Some()
{
    Assert.True(Maybe<int>.Some(10).Equals(Maybe<int>.Some(10)));
    Assert.True(Maybe<int>.Some(10) == Maybe<int>.Some(10));
    Assert.False(Maybe<int>.Some(10) != Maybe<int>.Some(10));
}
```

## Background : Algebraic Data Types 
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

Alternately, one could use this library to model union-types without switching languages.
