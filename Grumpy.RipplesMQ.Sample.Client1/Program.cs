using System.Threading;

namespace Grumpy.RipplesMQ.Sample.Client1
{
    public static class Program
    {
        private static void Main()
        {
            Thread.Sleep(5000);

            using (var client = new Tester())
            {
                client.Execute();
            }
        }
    }
}
