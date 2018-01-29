using System.Threading;
using Grumpy.ServiceBase;

namespace Grumpy.RipplesMQ.Sample.Server3
{
    public static class Program
    {
        private static void Main()
        {
            Thread.Sleep(5000);

            TopshelfUtility.Run<Server3Service>();
        }
    }
}
