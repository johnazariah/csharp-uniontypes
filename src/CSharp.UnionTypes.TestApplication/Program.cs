using System.Text.RegularExpressions;

namespace CSharp.UnionTypes.TestApplication
{
    public static class Program
    {
        public static void Main (string[] args)
        {
            Maybe<int> m23 = new Maybe<int>.Some(23);

            var str = m23 switch
            {
                Maybe<int>.Some { Value: var v } => $"Some {v}",
                Maybe<int>.None => "None",
                _ => throw new NotImplementedException()
            };

            Console.WriteLine(str);
            Console.WriteLine($"{m23}");

            var red = new TrafficLights.Red();
            var stopRed = (TrafficLightsToStopFor.Red)red;
            Console.WriteLine($"{red}, {stopRed}, {(TrafficLights.Red)stopRed}");

            var card = new PaymentMethod<string, int>.Card("1234");
            AuditablePaymentMethod<string, int>.Card auditableCard = card;
            Console.WriteLine($"{card}, {auditableCard}, {(PaymentMethod<string, int>.Card)auditableCard}");
        }
    }
}
