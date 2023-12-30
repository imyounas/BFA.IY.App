using BF.IY.Common;
using BF.IY.Common.Model;
using BF.IY.P2P.Node.Domain.Service;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BF.IY.P2P.Node.GRPCServices
{
    public class IncomingMessageProcessor : Auctioner.AuctionerBase
    {
    
        public override async Task MessageGateway(IAsyncStreamReader<ClientRequest> requestStream, IServerStreamWriter<ServerResponse> responseStream, 
            ServerCallContext context)
        {
            bool keepServerRunning = true;

            while (keepServerRunning)
            {
                try
                {
                    while (await requestStream.MoveNext(CancellationToken.None))
                    {

                        var clientMessage = requestStream.Current;

                        switch (clientMessage.RequestCase)
                        {
                            case ClientRequest.RequestOneofCase.JoinAuction:
                                {
                                    var clientJoinRequest = clientMessage.JoinAuction;
                                    await AuctionManager.AddNewPeerByClientId(clientJoinRequest.ClientId);

                                    Consoler.ServerMessageWriter($"\nTotal Nodes connected: [{AuctionManager.peerClients.Count}]");

                                }
                                break;

                            case ClientRequest.RequestOneofCase.AuctionCreateRequest:
                                {
                                    var auctionCreateRequest = clientMessage.AuctionCreateRequest;
                                    AuctionInfo auction = new AuctionInfo()
                                    {
                                        AuctionName = auctionCreateRequest.AuctionName,                                       
                                        ItemName = auctionCreateRequest.ItemName,
                                        ItemPrice = auctionCreateRequest.ItemPrice,
                                        ClientId = auctionCreateRequest.ClientId,
                                    };

                                    AuctionManager.AddNewAuction(auction);

                                    Consoler.ServerMessageWriter($"\nA New Auction has been created by the ClientId: [{auction.ClientId}] => \n\t\tAuction Name:{auction.AuctionName}\n\t\tItem Name:{auction.ItemName}\n\t\tPrice:{auction.ItemPrice}");

                                }
                                break;

                            case ClientRequest.RequestOneofCase.BidRequest:
                                {
                                    var bid = clientMessage.BidRequest;
                                    AuctionBidInfo bidReq = new AuctionBidInfo()
                                    {
                                        AuctionName = bid.AuctionName,
                                        ItemName = bid.ItemName,
                                        ItemPrice = bid.ItemPrice,
                                        BiddingClientId = bid.BiddingClientId,
                                        BidPrice = bid.BidPrice,
                                        AauctionCreaterClientId = bid.AuctionCreaterClientId,

                                    };

                                    AuctionManager.bids.Add(bidReq);

                                    Consoler.ServerMessageWriter($"\nA New Bid Request has been raised by the ClientId: [{bidReq.BiddingClientId}] => \n\t\tAuction Name:{bidReq.AuctionName}\n\t\tItem Name:{bidReq.ItemName}\n\t\tOriginal Price:{bidReq.ItemPrice}\n\t\tBid Price:{bidReq.BidPrice}");

                                }
                                break;

                            case ClientRequest.RequestOneofCase.BidAcceptedRequest:
                                {
                                    var bidAccepted = clientMessage.BidAcceptedRequest;
                                    AuctionManager.RemoveAuctionByName(bidAccepted.AuctionName);
                                    AuctionManager.RemoveBidByName(bidAccepted.AuctionName);

                                    Consoler.ServerMessageWriter($"\nA Bid has been accpted from the ClientId: [{bidAccepted.BiddingClientId}] => \n\t\tAuction Name:{bidAccepted.AuctionName}\n\t\tAccepted Price:{bidAccepted.AcceptedPrice}");

                                    if(AuctionManager.currentClient.Id == bidAccepted.BiddingClientId)
                                    {
                                        Consoler.ServerMessageWriter($"\n\t\t**** Congratulations you have won the Bid ****");
                                    }

                                }
                                break;

                            case ClientRequest.RequestOneofCase.AuctionCloseRequest:
                                {   

                                }
                                break;

                            default:
                                {
                                    Consoler.ErrorWriter($"\nUnknown Message Type Received {clientMessage}");
                                }
                                break;
                        }

                    }

                }
                catch (RpcException rpc)
                {
                  Consoler.ErrorWriter(rpc.Message);
                }
                catch (Exception ex)
                {
                    Consoler.ErrorWriter(ex.Message);
                }
            }
        }
    }
}
