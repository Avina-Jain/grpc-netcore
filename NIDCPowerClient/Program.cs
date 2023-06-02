using System;
using Grpc.Core;
using NationalInstruments.Grpc.DCPower;
using NationalInstruments.Grpc.Device;

namespace NIDCPowerClient
{
    class Program
    {
        public static void Main(string[] args)
        {
            var server_address = "localhost";
            var server_port = "31763";
            var session_name = "NI-DCPower-Session";

            // Resource name, channel name, and options for a simulated 4147 client.
            var resource = "SimulatedDCPower";
            var options = "Simulate=1,DriverSetup=Model:4147;BoardType:PXIe";
            var channels = "0";

            Channel channel = new Channel(server_address + ":" + server_port, ChannelCredentials.Insecure);

            var client = new NiDCPower.NiDCPowerClient(channel);

            var initialize_reply = client.InitializeWithChannels(new InitializeWithChannelsRequest
            {
                SessionName = session_name,
                ResourceName = resource,
                Channels = channels,
                Reset = false,
                OptionString = options
            });
            var vi = initialize_reply.Vi;
            if (initialize_reply.Status == 0)
            {
                Console.WriteLine("Initialization was successful.");
            }
            else
            {
                Console.WriteLine($"Initialization was not successful. Status is {initialize_reply.Status}");
            }

            try
            {
                // Create a request message for the EnumerateDevices RPC
                var request = new EnumerateDevicesRequest();

                // Call the EnumerateDevices RPC and get the response message
                var devicesClient = new NationalInstruments.Grpc.Device.SessionUtilities.SessionUtilitiesClient(channel);
                var response = devicesClient.EnumerateDevices(request);

                // Print the list of devices
                // Print the list of devices
                foreach (var device in response.Devices)
                {
                    Console.WriteLine($"Device Name: {device.Name}");
                    Console.WriteLine($"Device Model: {device.Model}");
                    Console.WriteLine($"Device Serial Number: {device.SerialNumber}");
                    Console.WriteLine($"Device Product Id: {device.ProductId}");
                    Console.WriteLine();
                }
            }
            catch (RpcException e)
            {
                Console.WriteLine("RPC failed: " + e);
            }

            client.Close(new CloseRequest
            {
                Vi = vi
            });

            channel.ShutdownAsync().Wait();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}