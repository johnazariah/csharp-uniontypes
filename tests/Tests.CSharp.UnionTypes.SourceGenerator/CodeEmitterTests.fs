module CSharp.UnionTypes.SourceGenerator.CodeEmitterTests

open CSharp.UnionTypes.CodeEmitter
open Xunit

[<Fact>]
let ``Maybe<T> is generated correctly`` () =
    let actual =
        """namespace CSharp.UnionTypes.TestApplication
{
	union Maybe<T> { Some<T> | None };
}"""
        |> GenerateNamespaceCode

    let expected =
        """namespace CSharp.UnionTypes.TestApplication
{
    public abstract partial record Maybe<T>
    {
        private Maybe() { }
        public sealed partial record Some(T Value) : Maybe<T>
        {
            override public string ToString() => $"Maybe<{typeof(T)}>.Some {Value}";
        }
        public sealed partial record None() : Maybe<T>
        {
            override public string ToString() => $"Maybe<{typeof(T)}>.None";
        }
    }
}
"""
    Assert.Equal (expected.Replace("\r\n", "\n"), actual.Replace("\r\n", "\n"))

[<Fact>]
let ``Constrains clause for enumerations is generated correctly`` () =
    let actual =
        """namespace CSharp.UnionTypes.TestApplication
{
	union TrafficLights { Red | Amber | Green };
	union TrafficLightsToStopFor constrains TrafficLights { Red | Amber };
}"""
        |> GenerateNamespaceCode

    let expected =
        """namespace CSharp.UnionTypes.TestApplication
{
    public abstract partial record TrafficLights
    {
        private TrafficLights() { }
        public sealed partial record Red() : TrafficLights
        {
            override public string ToString() => $"TrafficLights.Red";
        }
        public sealed partial record Amber() : TrafficLights
        {
            override public string ToString() => $"TrafficLights.Amber";
        }
        public sealed partial record Green() : TrafficLights
        {
            override public string ToString() => $"TrafficLights.Green";
        }
    }
    public abstract partial record TrafficLightsToStopFor
    {
        private TrafficLightsToStopFor() { }
        public sealed partial record Red() : TrafficLightsToStopFor
        {
            override public string ToString() => $"TrafficLightsToStopFor.Red";
            public static implicit operator Red(TrafficLights.Red value) => new Red();
            public static implicit operator TrafficLights.Red(Red value) => new TrafficLights.Red();
        }
        public sealed partial record Amber() : TrafficLightsToStopFor
        {
            override public string ToString() => $"TrafficLightsToStopFor.Amber";
            public static implicit operator Amber(TrafficLights.Amber value) => new Amber();
            public static implicit operator TrafficLights.Amber(Amber value) => new TrafficLights.Amber();
        }
    }
}
"""
    Assert.Equal (expected.Replace("\r\n", "\n"), actual.Replace("\r\n", "\n"))


[<Fact>]
let ``Constrains clause for value constructed unions is generated correctly`` () =
    let actual =
        """namespace CSharp.UnionTypes.TestApplication
{
	union PaymentMethod<TCard, TCheque> { Cash | Card<TCard> | Cheque<TCheque> };
	union AuditablePaymentMethod<TCard, TCheque> constrains PaymentMethod<TCard, TCheque> { Card<TCard> | Cheque<TCheque> };
}"""
        |> GenerateNamespaceCode

    let expected =
        """namespace CSharp.UnionTypes.TestApplication
{
    public abstract partial record PaymentMethod<TCard, TCheque>
    {
        private PaymentMethod() { }
        public sealed partial record Cash() : PaymentMethod<TCard, TCheque>
        {
            override public string ToString() => $"PaymentMethod<{typeof(TCard)}, {typeof(TCheque)}>.Cash";
        }
        public sealed partial record Card(TCard Value) : PaymentMethod<TCard, TCheque>
        {
            override public string ToString() => $"PaymentMethod<{typeof(TCard)}, {typeof(TCheque)}>.Card {Value}";
        }
        public sealed partial record Cheque(TCheque Value) : PaymentMethod<TCard, TCheque>
        {
            override public string ToString() => $"PaymentMethod<{typeof(TCard)}, {typeof(TCheque)}>.Cheque {Value}";
        }
    }
    public abstract partial record AuditablePaymentMethod<TCard, TCheque>
    {
        private AuditablePaymentMethod() { }
        public sealed partial record Card(TCard Value) : AuditablePaymentMethod<TCard, TCheque>
        {
            override public string ToString() => $"AuditablePaymentMethod<{typeof(TCard)}, {typeof(TCheque)}>.Card {Value}";
            public static implicit operator Card(PaymentMethod<TCard, TCheque>.Card value) => new Card(value.Value);
            public static implicit operator PaymentMethod<TCard, TCheque>.Card(Card value) => new PaymentMethod<TCard, TCheque>.Card(value.Value);
        }
        public sealed partial record Cheque(TCheque Value) : AuditablePaymentMethod<TCard, TCheque>
        {
            override public string ToString() => $"AuditablePaymentMethod<{typeof(TCard)}, {typeof(TCheque)}>.Cheque {Value}";
            public static implicit operator Cheque(PaymentMethod<TCard, TCheque>.Cheque value) => new Cheque(value.Value);
            public static implicit operator PaymentMethod<TCard, TCheque>.Cheque(Cheque value) => new PaymentMethod<TCard, TCheque>.Cheque(value.Value);
        }
    }
}
"""
    Assert.Equal (expected.Replace("\r\n", "\n"), actual.Replace("\r\n", "\n"))