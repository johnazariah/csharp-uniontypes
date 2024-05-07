namespace CSharp.UnionTypes.SourceGenerator
{
    namespace Samples.Maybe
    {
        using System;

        public static class Foo
        {
            private static int Match<T>(this Maybe<T> m) => m switch
            {
                Maybe<int>.Some s => s.Value,
                Maybe<int>.None => -1,
                _ => throw new NotImplementedException()
            };

            public static int Main() => new Maybe<int>.Some(23).Match();
        }

        // namespace Payment {
        //      union MoneyTransfer { MoneyTransfer<int> }
        //      union CreditCard    { CreditCard<int>    }
        //      union Payment       { Cash<double> | MoneyTransfer | CreditCard }
        // }

        // union Either<T, R> { Left<T> | Right<R> }
        public abstract partial record Either<T, R>
        {
            private Either() { }

            public sealed partial record Left(T Value) : Either<T, R>;
            public sealed partial record Right(R Value) : Either<T, R>;
        }

        // union Maybe<T> { Some<T> | None }
        public abstract partial record Maybe<T>
        {
            private Maybe() { }

            public sealed partial record Some(T Value) : Maybe<T>;
            public sealed partial record None() : Maybe<T>;
        }
    }
}
