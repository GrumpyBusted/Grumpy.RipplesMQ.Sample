using System;
using System.Configuration;
using System.Threading;
using Grumpy.Common.Extensions;
using Grumpy.RipplesMQ.Core.Interfaces;
using Grumpy.RipplesMQ.Server;
using Grumpy.ServiceBase;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Grumpy.RipplesMQ.Sample.Broker1
{
    public class MessageBrokerService : TopshelfServiceBase, IDisposable
    {
        private IMessageBroker _messageBroker;
        private LogLevel _logLevel = LogLevel.Warning;
        private bool _disposed;

        protected override void Process(CancellationToken cancellationToken)
        {
            Logger = new ConsoleLogger(ServiceName, (message, level) => level >= _logLevel, false);
            
            var appSettings = ConfigurationManager.AppSettings;

            if (Enum.TryParse(appSettings["LogLevel"], true, out LogLevel logLevel))
                _logLevel = logLevel;

            var messageBrokerBuilder = new MessageBrokerBuilder().WithServiceName(ServiceName).WithLogger(Logger);

            if (!appSettings["DatabaseServer"].NullOrEmpty())
                messageBrokerBuilder = messageBrokerBuilder.WithRepository(appSettings["DatabaseServer"], appSettings["DatabaseName"]);

            _messageBroker = messageBrokerBuilder.Build();

            _messageBroker.Start(cancellationToken);

            Console.WriteLine("RipplesMQ Message Broker Started");
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
                    _messageBroker?.Dispose();

                base.Dispose(disposing);
            }
        }
    }
}