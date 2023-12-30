using Ardalis.GuardClauses;
using BF.IY.Common.Model;
using Pastel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BF.IY.Common
{
    public static class Consoler
    {
        static Color inputColor = Color.Black;
        static Color yellowColor = Color.Yellow;
        static Color messageColor = Color.Green;
        static Color errorgeColor = Color.Red;
        static Color serverMessageColor = Color.BlueViolet;


        public static void ServerMessageWriter(string message)
        {
            Console.WriteLine($"\t\t{message}".Pastel(serverMessageColor));
        }

        public static void MessageWriterWithColor(string message, Color colr)
        {
            Console.WriteLine($"\t\t{message}".Pastel(colr));
        }


        public static void ClientMessageWriter(string message ,  bool newLine = true, bool isMenu = false)
        {
            Color color = isMenu ?  yellowColor : messageColor;
            if (newLine)
            {
                Console.WriteLine(message.Pastel(color));
            }
            else
            {
                Console.Write(message.Pastel(color));
            }
        }

        public static void ErrorWriter(string message)
        {
            Console.WriteLine(message.Pastel(errorgeColor));
        }

        public static ClientAuctionOption GetAuctionMenuAction()
        {
            bool allDone = false;
            ClientAuctionOption option = ClientAuctionOption.Unknown;
            while (!allDone)
            {
                try
                {
                    ClientMessageWriter("\n-------------------------Peer Network Auction Menue------------------------", isMenu:true);
                    ClientMessageWriter("\t1 - Create Auction", isMenu: true);
                    ClientMessageWriter("\t2 - Bid On Auction", isMenu: true);
                    ClientMessageWriter("\t3 - Accept Bid On Your Auction", isMenu: true);
                    //ClientMessageWriter("\t4 - Close Your Auction", isMenu: true);
                    //ClientMessageWriter("\t9 - Leave Auction Server", isMenu: true);
                    ClientMessageWriter("\n\tYour input: ", false, isMenu: true);

                    string? menuValue = Console.ReadLine();
                    Guard.Against.NullOrWhiteSpace(menuValue, "Menue Option", "Please provide valid value");
                    Guard.Against.InvalidInput(menuValue, "Menue Option", (val) => val == "1" || val == "2" || val == "3", "Value should be from Menue Options");
                    allDone = true;
                    var mv = int.Parse(menuValue);
                    option = (ClientAuctionOption)mv;

                    allDone = true;
                }
                catch (Exception ex)
                {
                    ErrorWriter(ex.Message);
                    ErrorWriter($"Let's try again...");
                }
            }

            return option;
        }

        public static int PrintAllAuctionsExceptMine(int currentClientId, List<AuctionInfo> allAuctions)
        {
            int actCount = 0;
            ClientMessageWriter($"\tAvailable Auctions:");
            foreach (var auc in allAuctions)
            {
                if (auc.ClientId != currentClientId)
                {
                    actCount++;
                    ClientMessageWriter($"\t* Name:[{auc.AuctionName}] - Item:[{auc.ItemName}] - Price:[{auc.ItemPrice}]");
                }
            }

            
            return actCount;
        }


        public static AuctionBidInfo BidOnAuction(int currentClientId, List<AuctionInfo> allAuctions)
        {
            bool allDone = false;
            string auctionName = string.Empty;
            double price = 0;
            AuctionBidInfo bidInfo = null;
            int maxRetry = 10;
            int retries = 0;
            while (!allDone && retries < maxRetry)
            {
                try
                {
                    ClientMessageWriter($"Enter Auction Name to Bid: ", false);
                    auctionName = Console.ReadLine()!;
                    Guard.Against.NullOrWhiteSpace(auctionName, "Auction Name", "Please provide valid Auction Name");
                    
                    var selectedAuction = allAuctions.Where(a=> string.Equals(a.AuctionName, auctionName, StringComparison.OrdinalIgnoreCase) && a.ClientId != currentClientId).FirstOrDefault();
                    
                    if(selectedAuction == null)
                    {
                        throw new Exception("Provided Auction does not exist");
                    }


                    ClientMessageWriter($"Enter Item Price to Bid: ", false);
                    string auctionPrice = Console.ReadLine()!;
                    Guard.Against.NullOrWhiteSpace(auctionPrice, " Item Price", "Please provide valid  Item Price");     
                    bool isconverted = double.TryParse(auctionPrice, out price);
                    if (!isconverted || price <= selectedAuction.ItemPrice) 
                    {
                        throw new Exception("Please provide Auction item price greater than existing price");
                    }


                    bidInfo = new AuctionBidInfo()
                    {
                        AuctionName = selectedAuction.AuctionName,
                        BidPrice = price,
                        ItemPrice = selectedAuction.ItemPrice,
                        ItemName = selectedAuction.ItemName,
                        BiddingClientId = currentClientId,
                        AauctionCreaterClientId = selectedAuction.ClientId
                    };

                    allDone = true;

                }
                catch (Exception ex)
                {
                    ErrorWriter(ex.Message);
                    ClientMessageWriter($"Let's try again...");
                    retries++;
                }
            }

            return bidInfo;


        }

        public static AuctionBidInfo AcceptBidsCreatedByMe(int currentClientId, List<AuctionBidInfo> bids)
        {
            int i = 1;
            AuctionBidInfo anyAcceptedBid = null;
            ClientMessageWriter($"\tAvailable Bids:");
            foreach (var auc in bids)
            {
                if (auc.AauctionCreaterClientId == currentClientId)
                {
                    auc.InternalId = i;
                    ClientMessageWriter($"\t[{i}]: Name:[{auc.AuctionName}] - Item:[{auc.ItemName}] - Price:[{auc.ItemPrice}] - Bidding Price:[{auc.BidPrice}] - Bidding ClientId:[{auc.BiddingClientId}]");
                    i++;
                }
            }

            if (i > 1)
            {
                ClientMessageWriter($"Enter Bid No. to accept , or any key to wait for better price: ", false);
                string bidNostr = string.Empty;
                bidNostr = Console.ReadLine()!;
                int bidNo = 0;
                if (bidNostr != string.Empty && int.TryParse(bidNostr, out bidNo))
                {
                    anyAcceptedBid = bids.Where(b => b.InternalId == bidNo).FirstOrDefault();

                }
            }

            return anyAcceptedBid;
        }
        public static AuctionInfo CreateAuction(int currentClientId)
        {
            bool allDone = false;           
            AuctionInfo auction = new AuctionInfo();
            int maxRetry = 10;
            int retries = 0;
            while (!allDone && retries < maxRetry)
            {
                try
                {
                    ClientMessageWriter($"Enter Auction Name: ", false);
                    auction.AuctionName = Console.ReadLine()!;
                    Guard.Against.NullOrWhiteSpace(auction.AuctionName, "Auction Name", "Please provide valid Auction Name");

                    ClientMessageWriter($"Enter Item Name: ", false);
                    auction.ItemName = Console.ReadLine()!;
                    Guard.Against.NullOrWhiteSpace(auction.ItemName, "Item Name", "Please provide valid Item Name");

                    ClientMessageWriter($"Enter Item Price: ", false);
                    string itemPrice = Console.ReadLine()!;
                    Guard.Against.NullOrWhiteSpace(itemPrice, "Item Price", "Please provide valid Item Price");

                    double itemPriceDouble = 0;
                    bool isconverted = double.TryParse(itemPrice, out itemPriceDouble);
                    if (!isconverted || itemPriceDouble <= 0.0)
                    {
                        throw new Exception("Please provide item valid price greater than 0");
                    }
                    auction.ItemPrice = itemPriceDouble;
                    auction.ClientId = currentClientId;

                    allDone = true;
                }
                catch (Exception ex)
                {
                    ErrorWriter(ex.Message);
                    ClientMessageWriter($"Let's try again...");
                    retries++;
                }
            }

            return auction;


        }

        public static ClientInfo ClientInput()
        {
            ClientInfo info = new ClientInfo();
            try
            {  
                info.Name = ClientNameInput();
                info.ServicePort = ServericePortInput();

            }
            catch (Exception ex)
            {
                ErrorWriter(ex.Message);
            }

            return info;
        }


        public static string ClientNameInput()
        {
            bool allDone = false;
            string clientName = string.Empty;
            while (!allDone)
            {
                try
                {
                    ClientMessageWriter($"Please enter your Name (between 3 to 15 chars)): ", false);
                    //clientName = Environment.GetEnvironmentVariable("ClientName")!;
                    if (string.IsNullOrWhiteSpace(clientName))
                    {
                        clientName = Console.ReadLine()!;
                    }
                    Guard.Against.NullOrWhiteSpace(clientName, "Client Name", "Please provide valid Client Name");
                    Guard.Against.InvalidInput(clientName, "Client Name", (name) => name.Length >= 3 || name.Length <= 15, "Client name should be between 3 to 15 characters");
                    allDone = true;

                }
                catch (Exception ex)
                {
                    ErrorWriter(ex.Message);
                    ClientMessageWriter($"Let's try again...");
                }
            }
            return clientName;
        }

        public static int ServericePortInput()
        {
            bool allDone = false;
            int nodePort = 5001;
            string nodePortStr = string.Empty;
            while (!allDone)
            {
                try
                {
                    ClientMessageWriter($"Please enter Port to be used for this Node (between 5001 to 7000): ", false);                   

                    //serverUrl = Environment.GetEnvironmentVariable("ServerUrl")!;
                    if (string.IsNullOrWhiteSpace(nodePortStr))
                    {
                        nodePortStr = Console.ReadLine()!;
                    }
                    Guard.Against.NullOrWhiteSpace(nodePortStr, "Node Port", "Please provide valid Node Port");
                    bool isdone = int.TryParse(nodePortStr, out nodePort);

                    if (!isdone)
                    {
                        throw new Exception("Please provide numeric value for Node Port between 5001 to 7000");
                    }

                    Guard.Against.InvalidInput(nodePort, "Node Port", (port) => port >= 5001 || port <= 7000, "Node Port should be between 5001 to 7000");
                    allDone = true;

                }
                catch (Exception ex)
                {
                    ErrorWriter(ex.Message);
                    ClientMessageWriter($"Let's try again...");
                }
            }

            return nodePort;
        }

        public static bool CleanServiceRegistery()
        {
            bool allDone = false;
            string clearReg = string.Empty;
            bool clear = false;
            while (!allDone)
            {
                try
                {
                    ClientMessageWriter($"Do you want to clear existing Service Registery[Only do this for first client at start of a Run] (y/Y) ?: ", false);
                    //clientName = Environment.GetEnvironmentVariable("ClientName")!;
                    if (string.IsNullOrWhiteSpace(clearReg))
                    {
                        clearReg = Console.ReadLine()!;
                    }

                    if(clearReg == "y" || clearReg == "Y")
                    {
                        clear = true;
                    }

                    allDone = true;

                }
                catch (Exception ex)
                {
                    ErrorWriter(ex.Message);
                    ClientMessageWriter($"Let's try again...");
                }
            }
            return clear;
        }

    }
}
