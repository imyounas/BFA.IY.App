using BF.IY.Common.Model;
using BF.IY.Common;
using Pastel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientServiceRegistey;

namespace BF.IY.P2P.Node
{
    public class AppDriver
    {
        ClientInfo theClient = null!;
       public async Task StartAuction()
        {  
            Console.WriteLine($"Welcome to Auction".Pastel(Color.White).PastelBg(Color.Green));

            try
            {
                theClient = Consoler.ClientInput();
                bool toClear = Consoler.CleanServiceRegistery();
                if(toClear)
                {
                    ServiceRegistery.CleanServiceRegistery();
                }

                // starting the listener
                var listenrTask = Task.Run(async () =>
                {
                    AuctionServer.StartGRPCServer(theClient.ServicePort);
                });

                AuctionClient auctionClient = new AuctionClient(theClient);
                await auctionClient.StartClient();

                await listenrTask;
            }
            catch (Exception ex)
            {
                Consoler.ErrorWriter(ex.Message);
            }
        }
    }
}
