using System;
using System.Configuration;
using System.Threading;
using Grumpy.Json;
using Grumpy.RipplesMQ.Client;
using Grumpy.RipplesMQ.Client.Interfaces;
using Grumpy.RipplesMQ.Sample.API;
using Grumpy.RipplesMQ.Sample.API.DTOs;
using Grumpy.ServiceBase;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Grumpy.RipplesMQ.Sample.Server1
{
    public class Server1Service : TopshelfServiceBase, IDisposable
    {
        private IMessageBus _messageBus;
        private LogLevel _logLevel = LogLevel.Warning;
        private bool _disposed;
        private static int _personCreatedCount;
        private static int _tripCreatedCount;
        private static int _requestPersonCount;

        protected override void Process(CancellationToken cancellationToken)
        {
            Logger = new ConsoleLogger(ServiceName, (message, level) => level >= _logLevel, false);
            
            var appSettings = ConfigurationManager.AppSettings;

            if (Enum.TryParse(appSettings["LogLevel"], true, out LogLevel logLevel))
                _logLevel = logLevel;

            _messageBus = new MessageBusBuilder().WithServiceName(ServiceName).WithLogger(Logger).Build();

            _messageBus.SubscribeHandler<PersonDto>(SampleApiConfiguration.PersonCreated, HandlePersonCreated, "HandlePersonCreated", true, true);
            _messageBus.SubscribeHandler<TripDto>(SampleApiConfiguration.TripCreated, HandleTripCreated, "HandleTripCreated", true, true);
            _messageBus.RequestHandler<PersonKeyDto, PersonDto>(SampleApiConfiguration.Person, PersonHandler, true);

            _messageBus.Start(cancellationToken);

            Console.WriteLine("RipplesMQ MessageBus Started");
        }

        public new void Dispose()
        {
            Dispose(true);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;

                if (disposing)
                    _messageBus?.Dispose();

                base.Dispose(disposing);
            }
        }

        private static void HandlePersonCreated(PersonDto dto, CancellationToken cancellationToken)
        {
            Console.WriteLine($"HandlePersonCreated {++_personCreatedCount}: " + dto.SerializeToJson());
        }

        private static void HandleTripCreated(TripDto dto, CancellationToken cancellationToken)
        {
            Console.WriteLine($"HandleTripCreated {++_tripCreatedCount}: " + dto.SerializeToJson());
        }

        private static PersonDto PersonHandler(PersonKeyDto request, CancellationToken cancellationToken)
        {
            PersonDto response;

            Console.WriteLine($"Request Person {++_requestPersonCount}: " + request.SerializeToJson());

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

            Console.WriteLine("Response Person: " + response.SerializeToJson());

            return response;
        }
    }
}
