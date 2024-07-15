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
            Console.WriteLine($"{new Result<int, Exception>.Return(18)}");
        }
    }
}
