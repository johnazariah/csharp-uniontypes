using System;
using CoolMonads;

namespace CoolMonads
{
    public partial class Maybe<T>
    {
        // Functor
        public Maybe<R> Map<R>(Func<T, R> f) => Match(() => Maybe<R>.None, _ => Maybe<R>.NewSome(f(_)));

        // Applicative
        public static Maybe<Y> Apply<X, Y>(Maybe<Func<X, Y>> f, Maybe<X> x) => f.Match(() => Maybe<Y>.None, _f => x.Match(() => Maybe<Y>.None, _x => Maybe<Y>.NewSome(_f(_x))));

        // Foldable
        public R Fold<R>(R z, Func<R, T, R> f) => Match(() => z, _ => f(z, _));

        // Monad
        public static Maybe<T> Unit(T value) => NewSome(value);
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
}

namespace CSharp.UnionTypes.GenerationSite
{
    class Program
    {
        static void Main(string[] args)
        {
            var stringOpt = Maybe<string>.NewSome("Hello, World");

            Console.WriteLine($"The length of the given string is {stringOpt.Match(() => 0, _ => _.Length)}");
            Console.WriteLine($"The length of the given string is {stringOpt.Map(_ => _.Length).Fold(0, (_, x) => x)}");

            var lenOpt = from s in Maybe<string>.NewSome("Hello, World")
                         select s.Length;
            Console.WriteLine($"The length of the given string is {lenOpt.GetOrElse(0)}");


            var lenOpt2 = from s1 in Maybe<string>.None
                          select s1.Length;
            Console.WriteLine($"The length of the given string is {lenOpt2.GetOrElse(0)}");
        }
    }
}