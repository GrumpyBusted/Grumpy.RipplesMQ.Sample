using Grumpy.ServiceBase;

namespace Grumpy.RipplesMQ.Sample.Server2
{
    public static class Program
    {
        private static void Main()
        {
            TopshelfUtility.Run<Server2Service>();
        }
    }
}
