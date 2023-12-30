using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BF.IY.Common.Model
{
    public class AuctionBidInfo
    {
        public string AuctionName { get; set; }
        public string ItemName { get; set; }
        public int AauctionCreaterClientId { get; set; }
        public int BiddingClientId { get; set; }
        public double ItemPrice { get; set; }
        public double BidPrice { get; set; }

        public int InternalId { get; set; }

    }
}
