using System;

namespace Grumpy.RipplesMQ.Sample.Client1
{
    public static class Program
    {
        private static void Main()
        {
            Console.WriteLine("Press key to start");
            Console.ReadKey(true);

            using (var client = new Tester())
            {
                client.Execute();
            }
        }
    }
}
