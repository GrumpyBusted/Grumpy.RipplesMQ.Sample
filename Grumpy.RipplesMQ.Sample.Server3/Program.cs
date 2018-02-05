using Grumpy.ServiceBase;

namespace Grumpy.RipplesMQ.Sample.Server3
{
    public static class Program
    {
        private static void Main()
        {
            TopshelfUtility.Run<Server3Service>();
        }
    }
}
