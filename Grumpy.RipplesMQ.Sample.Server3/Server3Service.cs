using System;
using System.Threading;
using Grumpy.Json;
using Grumpy.RipplesMQ.Client;
using Grumpy.RipplesMQ.Client.Interfaces;
using Grumpy.RipplesMQ.Sample.API;
using Grumpy.RipplesMQ.Sample.API.DTOs;
using Grumpy.ServiceBase;

namespace Grumpy.RipplesMQ.Sample.Server3
{
    public class Server3Service : TopshelfServiceBase
    {
        private IMessageBus _messageBus;

        protected override void Process(CancellationToken cancellationToken)
        {
            _messageBus = new MessageBusBuilder().Build();

            _messageBus.SubscribeHandler<PersonDto>(SampleApiConfiguration.PersonCreated, NotifyPerson, "NotifyPerson", true, true);
            _messageBus.SubscribeHandler<CarDto>(SampleApiConfiguration.CarCreated, HandleCarCreated, "HandleCarCreated", false, true);
            _messageBus.SubscribeHandler<TripDto>(SampleApiConfiguration.TripCreated, HandleTripCreated, "HandleTripCreated", true, true);

            _messageBus.Start(cancellationToken);
        }

        protected override void Clean()
        {
            _messageBus?.Dispose();
        }

        private static void NotifyPerson(PersonDto dto, CancellationToken cancellationToken)
        {
            Console.WriteLine("NotifyPerson: " + dto.SerializeToJson());
        }

        private static void HandleCarCreated(CarDto dto, CancellationToken cancellationToken)
        {
            Console.WriteLine("HandleCarCreated: " + dto.SerializeToJson());
        }

        private static void HandleTripCreated(TripDto dto, CancellationToken cancellationToken)
        {
            Console.WriteLine("HandleTripCreated: " + dto.SerializeToJson());
        }
    }
}
