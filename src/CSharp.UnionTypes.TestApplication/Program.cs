using System.Text.RegularExpressions;

namespace CSharp.UnionTypes.TestApplication
{
    public static class Program
    {
        //private record CCN(string Value);

        //private static void ProcessCreditCardPayment(CCN ccn)
        //{
        //    Console.WriteLine(ccn switch
        //    {
        //        CCN { Value: var v } => Assert.AreEqual("1234 5678 9012 3456", v),
        //        _ => "Invalid credit card number"
        //    });
        //}

        public static void Main (string[] args)
        {
            Maybe<int> m23 = new Maybe<int>.Some(23);

            Console.WriteLine(m23 switch
            {
                Maybe<int>.Some { Value: var v } => $"Some {v}",
                Maybe<int>.None => "None",
                _ => throw new NotImplementedException()
            });

            //var ccn = new CCN("1234 5678 9012 3456");
            //_ = ccn switch
            //{
            //    CCN { Value: var v } => Assert.AreEqual("1234 5678 9012 3456", v),
            //    _ => "Invalid credit card number"
            //};
        }
    }
}
