using BF.IY.P2P.Node.GRPCServices;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BF.IY.P2P.Node
{
    public static class AuctionServer
    {
        //const int Port = 50051;

        public static void StartGRPCServer(int port)
        {
            var serverStub = new IncomingMessageProcessor();

            Server server = new Server
            {
                Services = { Auctioner.BindService(serverStub) },
                Ports = { new ServerPort("localhost", port, ServerCredentials.Insecure) }
            };

            server.Start();

            Console.WriteLine($"Auctioner started on port {port}");

            var keyStroke = string.Empty;
            while (keyStroke != "q")
            {
               
            }

            server.ShutdownAsync().Wait();
        }
    }
}
