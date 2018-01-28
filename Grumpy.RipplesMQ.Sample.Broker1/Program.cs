using Grumpy.ServiceBase;

namespace Grumpy.RipplesMQ.Sample.Broker1
{
    public static class Program
    {
        private static void Main()
        {
            TopshelfUtility.Run<MessageBrokerService>();
        }
    }
}