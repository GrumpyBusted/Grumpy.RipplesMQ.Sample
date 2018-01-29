[![Build status](https://ci.appveyor.com/api/projects/status/u7htwvydrg7w0ryi?svg=true)](https://ci.appveyor.com/project/GrumpyBusted/grumpy-ripplesmq-sample)

# Grumpy.RipplesMQ.Sample
This is a sample solution using the RipplesMQ Message Broker. The Solution contain one client, three servers and two message 
brokers. This sample demostrate the cooperation of the two message broker servers, and how these distriture the work to the
servers providing functionality on the RipplesMQ Message Bus.

For more detail on setting up a RipplesMQ Message Broker server see 
[Grumpy.RipplesMQ.Server](https://github.com/GrumpyBusted/Grumpy.RipplesMQ.Server), see
[Grumpy.RipplesMQ.Client](https://github.com/GrumpyBusted/Grumpy.RipplesMQ.Client) to see how to setup a service using the
RipplesMQ Message Broker to provide or use functionality.

This sample is using a MS-SQL database for repository, connection string are configured in App.config of the Brokers.