using Grumpy.RipplesMQ.Config;

namespace Grumpy.RipplesMQ.Sample.API
{
    public static class SampleApiConfiguration 
    {
        public static readonly PublishSubscribeConfig PersonCreated = new PublishSubscribeConfig
        {
            Topic = "PersonCreated",
            Persistent = true
        };

        public static readonly PublishSubscribeConfig CarCreated = new PublishSubscribeConfig
        {
            Topic = "CarCreated",
            Persistent = true
        };

        public static readonly PublishSubscribeConfig TripCreated = new PublishSubscribeConfig
        {
            Topic = "TripCreated",
            Persistent = false
        };

        public static readonly RequestResponseConfig Person = new RequestResponseConfig
        {
            Name = "Person",
            MillisecondsTimeout = 100
        };
    }
}
