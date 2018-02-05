using Grumpy.ServiceBase;

namespace Grumpy.RipplesMQ.Sample.Server1
{
    public static class Program
    {
        private static void Main()
        {
            TopshelfUtility.Run<Server1Service>();
        }
    }
}
