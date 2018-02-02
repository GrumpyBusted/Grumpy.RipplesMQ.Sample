using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using Grumpy.Common;
using Grumpy.Common.Interfaces;
using Grumpy.Json;
using Grumpy.RipplesMQ.Client;
using Grumpy.RipplesMQ.Client.Interfaces;
using Grumpy.RipplesMQ.Config;
using Grumpy.RipplesMQ.Sample.API;
using Grumpy.RipplesMQ.Sample.API.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Grumpy.RipplesMQ.Sample.Client1
{
    internal class Tester : IDisposable
    {
        private readonly IMessageBus _messageBus;
        private readonly IProcessInformation _processInformation;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly LogLevel _logLevel = LogLevel.Warning;

        public Tester()
        {
            var appSettings = ConfigurationManager.AppSettings;

            if (Enum.TryParse(appSettings["LogLevel"], true, out LogLevel logLevel))
                _logLevel = logLevel;

            var logger = new ConsoleLogger("Tester", (message, level) => level >= _logLevel, false);
            var builder = new MessageBusBuilder().WithLogger(logger);

            _processInformation = new ProcessInformation();
            _cancellationTokenSource = new CancellationTokenSource();

            Console.WriteLine("Control+1: Publish Topic=PersonCreated Dto=PersonDto Persistent=true");
            Console.WriteLine("Control+2: Publish Topic=TripCreated Dto=TripDto Persistent=false");
            Console.WriteLine("Control+3: Request Name=Person");
            Console.WriteLine("Control+4: Publish Topic=CarCreated Dto=CarDto Persistent=true");
            Console.WriteLine($"The {_processInformation.ProcessName} tester is now running, press Control+C to exit.");

            _messageBus = builder.Build();
            _messageBus.Start(_cancellationTokenSource.Token);

            Console.WriteLine("RipplesMQ MessageBus Started");
        }

        public void Execute()
        {
            var inputString = "";

            Console.TreatControlCAsInput = true;

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                var key = Console.ReadKey(true);

                if ((key.Modifiers & ConsoleModifiers.Control) != 0 && key.Key == ConsoleKey.C)
                    Cancel();
                else if (key.Key == ConsoleKey.Delete)
                    Clear(out inputString);
                else if ((key.Modifiers & ConsoleModifiers.Control) != 0 && key.Key == ConsoleKey.D1)
                    PublishPersonCreated(ref inputString);
                else if ((key.Modifiers & ConsoleModifiers.Control) != 0 && key.Key == ConsoleKey.D2)
                    PublishTripCreated(ref inputString);
                else if ((key.Modifiers & ConsoleModifiers.Control) != 0 && key.Key == ConsoleKey.D3)
                    Request(ref inputString);
                else if ((key.Modifiers & ConsoleModifiers.Control) != 0 && key.Key == ConsoleKey.D4)
                    PublishCarCreated(ref inputString);
                else if ((key.Modifiers & ConsoleModifiers.Control) == 0 && (key.Modifiers & ConsoleModifiers.Alt) == 0 && key.Key != ConsoleKey.Backspace)
                    CaptureInput(ref inputString, key.KeyChar);
            }
        }

        private static void Clear(out string inputString)
        {
            Console.WriteLine();

            inputString = "";
        }

        private static void CaptureInput(ref string inputString, char key)
        {
            inputString += key;

            Console.Write(key);
        }

        private void PublishPersonCreated(ref string input)
        {
            var person = new PersonDto
            {
                Id = 99,
                Name = input,
                BirthDate = DateTime.Today
            };

            Publish(SampleApiConfiguration.PersonCreated, person);

            input = "";
        }

        private void PublishTripCreated(ref string input)
        {
            var trip = new TripDto
            {
                Car = input,
                Persons = new List<string>()
            };

            Publish(SampleApiConfiguration.TripCreated, trip);

            input = "";
        }

        private void PublishCarCreated(ref string input)
        {
            var car = new CarDto
            {
                Make = input
            };

            Publish(SampleApiConfiguration.CarCreated, car);

            input = "";
        }

        private void Publish<T>(PublishSubscribeConfig config, T message)
        {
            Console.WriteLine();
            Console.WriteLine($"Publish - Topic: {config.Topic}");
            Console.WriteLine("Message: " + message.SerializeToJson());

            try
            {
                _messageBus.Publish(config, message);
            }
            catch(Exception exception)
            {
                Console.WriteLine("Exception publishing: " + exception.SerializeToJson());
            }
        }

        private void Request(ref string input)
        {
            var request = new PersonKeyDto();

            int.TryParse(input, out request.Id);

            Request<PersonKeyDto, PersonDto>(SampleApiConfiguration.Person, request);

            input = "";
        }

        private void Request<TReq, TRes>(RequestResponseConfig config, TReq request)
        {
            Console.WriteLine();
            Console.WriteLine($"Request/Response - Name: {config.Name}");
            Console.WriteLine("Request: " + request.SerializeToJson());

            try
            {
                var response = _messageBus.Request<TReq, TRes>(config, request);
                Console.WriteLine("Response: " + response.SerializeToJson());
            }
            catch (Exception exception)
            {
                Console.WriteLine("Exception publishing: " + exception.SerializeToJson());
            }
        }

        private void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        public void Dispose()
        {
            _messageBus.Dispose();

            Console.WriteLine($"The {_processInformation.ProcessName} tester has stopped.");
        }
    }
}