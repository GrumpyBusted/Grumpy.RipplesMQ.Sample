using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Grumpy.Common;
using Grumpy.Common.Interfaces;
using Grumpy.Json;
using Grumpy.Logging;
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
        private readonly ILogger _logger;

        public Tester()
        {
            var appSettings = ConfigurationManager.AppSettings;

            if (Enum.TryParse(appSettings["LogLevel"], true, out LogLevel logLevel))
                _logLevel = logLevel;

            _logger = new ConsoleLogger("Tester", (message, level) => level >= _logLevel, false);
            var builder = new MessageBusBuilder().WithLogger(_logger);

            _processInformation = new ProcessInformation();
            _cancellationTokenSource = new CancellationTokenSource();

            Console.WriteLine("Alt+1: Publish Topic=PersonCreated Dto=PersonDto Persistent=true");
            Console.WriteLine("Alt+2: Publish Topic=TripCreated Dto=TripDto Persistent=false");
            Console.WriteLine("Alt+3: Request Name=Person");
            Console.WriteLine("Alt+4: Publish Topic=CarCreated Dto=CarDto Persistent=true");
            Console.WriteLine("Alt+5: RequestSync Name=Person with id 1 (n Times)");
            Console.WriteLine("Alt+6: RequestAsync Name=Person with id 1 (n Times)");
            Console.WriteLine("Alt+7: Publish Persistent Message (n Times)");
            Console.WriteLine("Alt+8: Publish Non-Persistent Message (n Times)");
            Console.WriteLine("Alt+9: Request NonExisting");
            Console.WriteLine($"The {_processInformation.ProcessName} tester is now running, press Control+C to exit.");

            try
            {
                _messageBus = builder.Build();
                _messageBus.Start(_cancellationTokenSource.Token);

                Console.WriteLine("RipplesMQ MessageBus Started");
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Exception {@Exception}", exception);
            }
        }

        public void Execute()
        {
            try
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
                    else if ((key.Modifiers & ConsoleModifiers.Alt) != 0 && key.Key == ConsoleKey.D1)
                        PublishPersonCreated(ref inputString);
                    else if ((key.Modifiers & ConsoleModifiers.Alt) != 0 && key.Key == ConsoleKey.D2)
                        PublishTripCreated(ref inputString);
                    else if ((key.Modifiers & ConsoleModifiers.Alt) != 0 && key.Key == ConsoleKey.D3)
                        RequestPerson(ref inputString);
                    else if ((key.Modifiers & ConsoleModifiers.Alt) != 0 && key.Key == ConsoleKey.D4)
                        PublishCarCreated(ref inputString);
                    else if ((key.Modifiers & ConsoleModifiers.Alt) != 0 && key.Key == ConsoleKey.D5)
                        RequestMulti(ref inputString);
                    else if ((key.Modifiers & ConsoleModifiers.Alt) != 0 && key.Key == ConsoleKey.D6)
                        RequestAsyncMulti(ref inputString);
                    else if ((key.Modifiers & ConsoleModifiers.Alt) != 0 && key.Key == ConsoleKey.D7)
                        PublishPersistent(ref inputString);
                    else if ((key.Modifiers & ConsoleModifiers.Alt) != 0 && key.Key == ConsoleKey.D8)
                        PublishNonPersistent(ref inputString);
                    else if ((key.Modifiers & ConsoleModifiers.Alt) != 0 && key.Key == ConsoleKey.D9)
                        RequestNonExisting(ref inputString);
                    else if ((key.Modifiers & ConsoleModifiers.Alt) != 0 && key.Key == ConsoleKey.D0)
                        PublishNonExisting(ref inputString);
                    else if ((key.Modifiers & ConsoleModifiers.Control) == 0 &&
                             (key.Modifiers & ConsoleModifiers.Alt) == 0 && key.Key != ConsoleKey.Backspace)
                        CaptureInput(ref inputString, key.KeyChar);
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Exception {@Exception}", exception);
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

        private void PublishNonExisting(ref string input)
        {
            Publish(SampleApiConfiguration.NonExistingCreated, input);

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
            catch (Exception exception)
            {
                Console.WriteLine("Exception publishing: " + exception.SerializeToJson());
            }
        }

        private void RequestPerson(ref string input)
        {
            var request = new PersonKeyDto();

            int.TryParse(input, out request.Id);

            Request<PersonKeyDto, PersonDto>(SampleApiConfiguration.Person, request);

            input = "";
        }

        private void RequestNonExisting(ref string input)
        {
            Request<string, string>(SampleApiConfiguration.NonExisting, input);

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
                Console.WriteLine("Exception requesting: " + exception.SerializeToJson());
            }
        }

        private void RequestMulti(ref string inputString)
        {
            Console.WriteLine();
            var request = new PersonKeyDto { Id = 1 };

            int.TryParse(inputString, out var count);

            Console.WriteLine($"Requesting Sync {count} times");

            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                for (var i = 0; i < count; ++i)
                {
                    var response = _messageBus.Request<PersonKeyDto, PersonDto>(SampleApiConfiguration.Person, request);

                    if (response == null)
                        throw new NullReferenceException("Missing request reply");
                }

                stopwatch.Stop();

                Console.WriteLine($"Request - Count: {count} - Total: {(double)stopwatch.ElapsedMilliseconds / 1000:#0.###}s - Average: {stopwatch.ElapsedMilliseconds / count}ms");
            }
            catch (Exception exception)
            {
                Console.WriteLine("Exception requesting: " + exception.SerializeToJson());
            }

            inputString = "";
        }

        private void RequestAsyncMulti(ref string inputString)
        {
            Console.WriteLine();

            var request = new PersonKeyDto { Id = 1 };

            int.TryParse(inputString, out var count);

            Console.WriteLine($"Requesting Async {count} times");

            try
            {
                var stopwatch = new Stopwatch();
                var responses = new List<Task<PersonDto>>();

                stopwatch.Start();

                for (var i = 0; i < count; ++i)
                    responses.Add(_messageBus.RequestAsync<PersonKeyDto, PersonDto>(SampleApiConfiguration.Person, request));

                Console.WriteLine($"RequestAsync Done - Count: {count} - Total: {(double)stopwatch.ElapsedMilliseconds / 1000:#0.###}s - Average: {stopwatch.ElapsedMilliseconds / count}ms");

                foreach (var response in responses)
                {
                    if (response == null)
                        throw new NullReferenceException("Missing request reply");

                    if (response.Result == null)
                        throw new NullReferenceException("Missing request reply");
                }

                stopwatch.Stop();

                Console.WriteLine($"Response from all - Count: {count} - Total: {(double)stopwatch.ElapsedMilliseconds / 1000:#0.###}s - Average: {stopwatch.ElapsedMilliseconds / count}ms");
            }
            catch (Exception exception)
            {
                Console.WriteLine("Exception requesting: " + exception.SerializeToJson());
            }

            inputString = "";
        }

        private void PublishPersistent(ref string inputString)
        {
            Console.WriteLine();

            var message = new PersonDto { Id = 0, Name = "Performance" };

            int.TryParse(inputString, out var count);

            Console.WriteLine($"Publish Persistent {count} times");

            try
            {
                var stopwatch = new Stopwatch();

                stopwatch.Start();

                for (var i = 0; i < count; ++i)
                    _messageBus.Publish(SampleApiConfiguration.PersonCreated, message);

                stopwatch.Stop();

                Console.WriteLine($"Publish Persistent - Count: {count} - Total: {(double)stopwatch.ElapsedMilliseconds / 1000:#0.###}s - Average: {stopwatch.ElapsedMilliseconds / count}ms");
            }
            catch (Exception exception)
            {
                Console.WriteLine("Exception publish: " + exception.SerializeToJson());
            }

            inputString = "";
        }

        private void PublishNonPersistent(ref string inputString)
        {
            Console.WriteLine();

            var message = new TripDto { Car = "Performance" };

            int.TryParse(inputString, out var count);

            Console.WriteLine($"Publish Non-Persistent {count} times");

            try
            {
                var stopwatch = new Stopwatch();

                stopwatch.Start();

                for (var i = 0; i < count; ++i)
                    _messageBus.Publish(SampleApiConfiguration.TripCreated, message);

                stopwatch.Stop();

                Console.WriteLine($"Publish Non-Persistent - Count: {count} - Total: {(double)stopwatch.ElapsedMilliseconds / 1000:#0.###}s - Average: {stopwatch.ElapsedMilliseconds / count}ms");
            }
            catch (Exception exception)
            {
                Console.WriteLine("Exception publish: " + exception.SerializeToJson());
            }

            inputString = "";
        }

        private void Cancel()
        {
            Console.WriteLine();
            Console.WriteLine("Control+C detected, attempting to stop service.");

            _cancellationTokenSource.Cancel();
        }

        public void Dispose()
        {
            _messageBus.Dispose();

            Console.WriteLine($"The {_processInformation.ProcessName} tester has stopped.");
        }
    }
}