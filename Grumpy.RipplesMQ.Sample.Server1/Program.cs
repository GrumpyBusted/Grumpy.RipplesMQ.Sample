using System.Threading;
using Grumpy.ServiceBase;

namespace Grumpy.RipplesMQ.Sample.Server1
{
    public static class Program
    {
        private static void Main()
        {
            Thread.Sleep(1000);

            TopshelfUtility.Run<Server1Service>();
        }
    }
}
