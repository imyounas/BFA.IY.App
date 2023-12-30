using BF.IY.Common;
using BF.IY.Common.Model;
using BF.IY.P2P.Node.Domain.Service;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Pastel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace BF.IY.P2P.Node
{
    public class AuctionClient
    {
        private readonly ClientInfo theClient;
        public AuctionClient(ClientInfo client) 
        {
            theClient = client;
        }
        public async Task StartClient()
        {
            try
            {
               
                Consoler.ClientMessageWriter($"Registering the Node {theClient.Name}");
                AuctionManager.RegisterAuctionClient( theClient );

                Consoler.MessageWriterWithColor($"\n\t***** This Node has been assigned Id: [{theClient.Id}] *****", Color.YellowGreen);

                Consoler.ClientMessageWriter($"Discovering existing Peer Nodes...");
                var clients = AuctionManager.GetAllRegisteredClients();
             
                await AuctionManager.InitPeerNetwork(clients);

                await ProcessClientRequests();
                
                Consoler.ClientMessageWriter($"All client work is done");
                Console.ReadLine();

            }
            catch (Exception ex)
            {
                Consoler.ErrorWriter(ex.Message);
            }
        }

        public async Task ProcessClientRequests()
        {
            bool leaveAuctionServer = false;
            Consoler.ClientMessageWriter("Awaiting other peers to join. I'll show menue once someone's join");
            while (!leaveAuctionServer)
            {
                // no peers then dont show menue
                if (AuctionManager.peerClients.Count <= 1)
                {  
                    await Task.Delay(1000);
                    continue;
                }

                Consoler.ClientMessageWriter($"<Total Nodes connected: [{AuctionManager.peerClients.Count}]>");
                var option = Consoler.GetAuctionMenuAction();
                switch (option)
                {
                    case ClientAuctionOption.CreateAuction:
                        {
                            var auction = Consoler.CreateAuction(theClient.Id);
                            if (auction != null && auction.ClientId > 0)
                            {
                                var auctionReq = new ClientRequest
                                {
                                    AuctionCreateRequest = new CreateAuctionRequest
                                    {
                                        ClientId = theClient.Id,
                                        ItemName = auction.ItemName,
                                        ItemPrice = auction.ItemPrice,
                                        AuctionName = auction.AuctionName
                                    }
                                };

                                await BroadcastToAllPeerNetwork(auctionReq);
                            }
                        }
                        break;

                    case ClientAuctionOption.BidOnAuction:
                        {
                           var acCount = Consoler.PrintAllAuctionsExceptMine(theClient.Id, AuctionManager.allAuctions);
                            if (acCount > 0)
                            {
                                var bid = Consoler.BidOnAuction(theClient.Id, AuctionManager.allAuctions);

                                var auctionReq = new ClientRequest
                                {
                                    BidRequest = new BidRequest
                                    {
                                        BiddingClientId = theClient.Id,
                                        ItemName = bid.ItemName,
                                        ItemPrice = bid.ItemPrice,
                                        AuctionName = bid.AuctionName,
                                        BidPrice = bid.BidPrice,
                                        AuctionCreaterClientId = bid.AauctionCreaterClientId
                                    }
                                };

                                await BroadcastToAllPeerNetwork(auctionReq);
                            }
                            else
                            {
                                Consoler.ClientMessageWriter($"\tNo Auctions available for you to bid");
                            }
                        }
                        break;

                    case ClientAuctionOption.AcceptAuction:
                        {
                            var acceptedBid = Consoler.AcceptBidsCreatedByMe(theClient.Id, AuctionManager.bids);
                            if(acceptedBid != null)
                            {

                                var auctionReq = new ClientRequest
                                {
                                    BidAcceptedRequest = new BidAcceptedRequest
                                    {
                                        BiddingClientId = acceptedBid.BiddingClientId,
                                        AcceptedPrice = acceptedBid.BidPrice,
                                        Accepted = true,
                                        AuctionName = acceptedBid.AuctionName
                                    }
                                };

                                await BroadcastToAllPeerNetwork(auctionReq);

                                await Task.Delay(1000);

                                var auctionCloseReq = new ClientRequest
                                {
                                    AuctionCloseRequest = new AuctionCloseRequest
                                    {  
                                        AuctionName = acceptedBid.AuctionName
                                    }
                                };

                                await BroadcastToAllPeerNetwork(auctionCloseReq);
                            }
                            else
                            {
                                Consoler.ClientMessageWriter($"\tNo Auctions Bids available for you");
                            }
                        }
                        break;
           
                    case ClientAuctionOption.Unknown:
                        {
                            Consoler.ErrorWriter("Receved Unknow Menue Option...");
                        }
                        break;
                }
            }
        }


        private void PrintAllAuctionsExceptMine()
        {   
            foreach (var auc in AuctionManager.allAuctions)
            {
                if(auc.ClientId != theClient.Id) 
                {
                    
                    Consoler.ClientMessageWriter($"\tName:[{auc.AuctionName}] - Item:[{auc.ItemName}] - Price:[{auc.ItemPrice}]");
                }
            }
        }

        private void BidOnAuction()
        {

        }


        private async Task<bool> BroadcastToAllPeerNetwork(ClientRequest request)
        {
            Consoler.ClientMessageWriter("Sending Auction Create Request...");

            foreach (var peerNodeKV in AuctionManager.peerStreams)
            {
                await peerNodeKV.Value.RequestStream.WriteAsync(request);

                Consoler.ClientMessageWriter($"Broadcast Message sent to Node [{peerNodeKV.Key}]");
            }

            return true;
        }

    }
}
