using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BF.IY.Common
{
    public enum ClientAuctionOption
    {
        LeaveAuctionServer = 9,
        CreateAuction = 1,
        BidOnAuction = 2,
        AcceptAuction = 3,
        ClosetAuction = 4,
        Unknown = -1
    }
}
