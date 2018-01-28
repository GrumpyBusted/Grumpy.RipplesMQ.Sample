using Grumpy.ServiceBase;

namespace Grumpy.RipplesMQ.Sample.Broker2
{
    public static class Program
    {
        private static void Main()
        {
            TopshelfUtility.Run<MessageBrokerService>();
        }
    }
}