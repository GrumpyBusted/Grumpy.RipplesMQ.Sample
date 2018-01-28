using System;
using System.Threading;
using Grumpy.Common;
using Grumpy.Common.Interfaces;
using Grumpy.Json;
using Grumpy.RipplesMQ.Client;
using Grumpy.RipplesMQ.Client.Interfaces;
using Grumpy.RipplesMQ.Config;
using Grumpy.RipplesMQ.Sample.API;
using Grumpy.RipplesMQ.Sample.API.DTOs;

namespace Grumpy.RipplesMQ.Sample.Client1
{
    internal class Tester : IDisposable
    {
        private readonly IMessageBus _messageBus;
        private readonly IProcessInformation _processInformation;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public Tester()
        {
            var builder = new MessageBusBuilder();
            _processInformation = new ProcessInformation();
            _cancellationTokenSource = new CancellationTokenSource();

            _messageBus = builder.Build();
            _messageBus.Start(_cancellationTokenSource.Token);

            Console.WriteLine("Control+1: Publish Topic=PersonCreated Dto=PersonDto");
            Console.WriteLine($"The {_processInformation.ProcessName} tester is now running, press Control+C to exit.");
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

        private void PublishPersonCreated(ref string name)
        {
            var person = new PersonDto
            {
                Id = 99,
                Name = name,
                BirthDate = DateTime.Today
            };

            Publish(SampleApiConfiguration.PersonCreated, person);

            name = "";
        }

        private void Publish(PublishSubscribeConfig config, object message)
        {
            Console.WriteLine();

            _messageBus.Publish(config, message);

            Console.WriteLine($"Publish - Topic: {config.Topic}");
            Console.WriteLine(message.SerializeToJson());
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