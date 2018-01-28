using System;
using System.Threading;
using Grumpy.Json;
using Grumpy.RipplesMQ.Client;
using Grumpy.RipplesMQ.Client.Interfaces;
using Grumpy.RipplesMQ.Sample.API;
using Grumpy.RipplesMQ.Sample.API.DTOs;
using Grumpy.ServiceBase;

namespace Grumpy.RipplesMQ.Sample.Server1
{
    public class Server1Service : TopshelfServiceBase
    {
        private IMessageBus _messageBus;

        protected override void Process(CancellationToken cancellationToken)
        {
            _messageBus = new MessageBusBuilder().Build();

            _messageBus.SubscribeHandler<PersonDto>(SampleApiConfiguration.PersonCreated, HandlePersonCreated, "HandlePersonCreated", true, true);
            _messageBus.SubscribeHandler<TripDto>(SampleApiConfiguration.TripCreated, HandleTripCreated, "HandleTripCreated", true, true);

            _messageBus.RequestHandler<PersonKeyDto, PersonDto>(SampleApiConfiguration.Person, PersonHandler, true);

            _messageBus.Start(cancellationToken);
        }

        protected override void Clean()
        {
            _messageBus?.Dispose();
        }

        private static void HandlePersonCreated(PersonDto dto, CancellationToken cancellationToken)
        {
            Console.WriteLine("HandlePersonCreated: " + dto.SerializeToJson());
        }

        private static void HandleTripCreated(TripDto dto, CancellationToken cancellationToken)
        {
            Console.WriteLine("HandleTripCreated: " + dto.SerializeToJson());
        }

        private static PersonDto PersonHandler(PersonKeyDto request, CancellationToken cancellationToken)
        {
            PersonDto response;

            Console.WriteLine("Request Person: " + request.SerializeToJson());

            switch (request.Id)
            {
                case 1:
                    response = new PersonDto { Id = request.Id, Name = "Monique", BirthDate = new DateTime(1975, 10, 5) };
                    break;
                case 2:
                    response = new PersonDto { Id = request.Id, Name = "Cathrine", BirthDate = new DateTime(2007, 6, 6) };
                    break;
                default:
                    response = null;
                    break;
            }

            Console.WriteLine("Response Person: " + response?.SerializeToJson());

            return response;
        }
    }
}
