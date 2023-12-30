using BF.IY.Common;
using BF.IY.Common.Model;
using ClientServiceRegistey;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BF.IY.P2P.Node.Domain.Service
{
    public static class AuctionManager
    {
        public static readonly List<ClientInfo> peerClients = new List<ClientInfo>();
        public static readonly Dictionary<int, AsyncDuplexStreamingCall<ClientRequest, ServerResponse>> peerStreams = new Dictionary<int, AsyncDuplexStreamingCall<ClientRequest, ServerResponse>>();
        public static readonly List<AuctionInfo> allAuctions = new List<AuctionInfo>();
        public static readonly List<AuctionBidInfo> bids = new List<AuctionBidInfo>();

        public static ClientInfo currentClient;

        public static bool RegisterAuctionClient(ClientInfo client)
        {
            var isRegistered = ServiceRegistery.RegisterClient(client);
            currentClient = client;
            return isRegistered;
        }

        public static ClientInfo GetAuctionClientById(int clientId)
        {
            var client = ServiceRegistery.GetClient(clientId);
            return client;
        }

        public static List<ClientInfo> GetAllRegisteredClients()
        {
            var clients = ServiceRegistery.GetAllRegisteredClients();
            return clients;
        }

        public static Task AddNewPeerByClientId(int newClientId)
        {
            var newClient = GetAuctionClientById(newClientId);
            peerClients.Add(newClient);

            string peerNodeUrl = $"http://localhost:{newClient.ServicePort}";

            var peerConnection = ConnectToPeer(peerNodeUrl);
            peerStreams.Add(newClient.Id, peerConnection);
            Consoler.ClientMessageWriter($"Successfully Connected to Peer [{newClient.Name}] on [{peerNodeUrl}]");

            return Task.CompletedTask;
        }


        public static bool AddNewAuction(AuctionInfo auction)
        {
            allAuctions.Add(auction);
            return true;
        }

        public static void RemoveAuctionByName(string auctionName)
        {
            allAuctions.RemoveAll(a => string.Equals(a.AuctionName, auctionName, StringComparison.OrdinalIgnoreCase));
        }

        public static bool AddNewBid(AuctionBidInfo bid)
        {
            bids.Add(bid);
            return true;
        }

        public static void RemoveBidByName(string auctionName)
        {
            bids.RemoveAll(a => string.Equals(a.AuctionName, auctionName, StringComparison.OrdinalIgnoreCase));

        }


        public static async Task InitPeerNetwork(List<ClientInfo> bsPeerClients)
        {
            peerClients.AddRange(bsPeerClients);
            await JoinPeerNetwork();
        }


        private static async Task JoinPeerNetwork()
        {
            foreach (var peer in peerClients)
            {
                if(peer.Id == currentClient.Id)
                {
                    continue;
                }

                try
                {
                    string peerNodeUrl = $"http://localhost:{peer.ServicePort}";

                    var peerConnection = ConnectToPeer(peerNodeUrl);
                    Consoler.ClientMessageWriter($"Sending Connection to Peer [{peer.Name}] on [{peerNodeUrl}]");
                    await peerConnection.RequestStream.WriteAsync(new ClientRequest
                    {
                        JoinAuction = new JoinAuction
                        {
                            ClientId = currentClient.Id,
                        }
                    });


                    peerStreams.Add(peer.Id, peerConnection);
                    Consoler.ClientMessageWriter($"Successfully Connected to Peer [{peer.Name}] on [{peerNodeUrl}]");


                }
                catch (Exception ex)
                {
                    Consoler.ErrorWriter(ex.Message);
                }
            }
        }

        private static AsyncDuplexStreamingCall<ClientRequest, ServerResponse> ConnectToPeer(string peerNodeUrl)
        {
            AsyncDuplexStreamingCall<ClientRequest,ServerResponse> peerConnection = null;
            try
            {

                var channel = GrpcChannel.ForAddress(peerNodeUrl);
                var peerNode = new Auctioner.AuctionerClient(channel);
                peerConnection = peerNode.MessageGateway();

                Consoler.ClientMessageWriter($"Connecting to Peer [{peerNodeUrl}]");

            }
            catch (Exception ex)
            {
                Consoler.ErrorWriter(ex.Message);
            }

            return peerConnection!;
        }
    }
}
