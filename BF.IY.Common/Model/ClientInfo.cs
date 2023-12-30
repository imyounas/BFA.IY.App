using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BF.IY.Common.Model
{
    public class ClientInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ServiceHost { get; set; } = "localhost";
        public int ServicePort { get; set; }
    }
}
