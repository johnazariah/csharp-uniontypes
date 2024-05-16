using System.Text.RegularExpressions;

namespace CSharp.UnionTypes.TestApplication
{
    public static class Program
    {

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
    }
}
