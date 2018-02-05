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

namespace Grumpy.RipplesMQ.Sample.Server3
{
    public class Server3Service : TopshelfServiceBase, IDisposable
    {
        private IMessageBus _messageBus;
        private LogLevel _logLevel = LogLevel.Warning;
        private bool _disposed;
        private static int _notifyPersonCount;
        private static int _carCreatedCount;
        private static int _tripCreatedCount;

        protected override void Process(CancellationToken cancellationToken)
        {
            Logger = new ConsoleLogger(ServiceName, (message, level) => level >= _logLevel, false);
            
            var appSettings = ConfigurationManager.AppSettings;

            if (Enum.TryParse(appSettings["LogLevel"], true, out LogLevel logLevel))
                _logLevel = logLevel;

            _messageBus = new MessageBusBuilder().WithServiceName(ServiceName).WithLogger(Logger).Build();

            _messageBus.SubscribeHandler<PersonDto>(SampleApiConfiguration.PersonCreated, NotifyPerson, "NotifyPerson", true, true);
            _messageBus.SubscribeHandler<CarDto>(SampleApiConfiguration.CarCreated, HandleCarCreated, "HandleCarCreated", false, true);
            _messageBus.SubscribeHandler<TripDto>(SampleApiConfiguration.TripCreated, HandleTripCreated, "HandleTripCreated", true, true);

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

        private static void NotifyPerson(PersonDto dto, CancellationToken cancellationToken)
        {
            Console.WriteLine($"NotifyPerson {++_notifyPersonCount}: " + dto.SerializeToJson());
        }

        private static void HandleCarCreated(CarDto dto, CancellationToken cancellationToken)
        {
            Console.WriteLine($"HandleCarCreated {++_carCreatedCount}: " + dto.SerializeToJson());
        }

        private static void HandleTripCreated(TripDto dto, CancellationToken cancellationToken)
        {
            Console.WriteLine($"HandleTripCreated {++_tripCreatedCount}: " + dto.SerializeToJson());
        }
    }
}
