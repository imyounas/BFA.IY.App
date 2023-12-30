using BF.IY.Common.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientServiceRegistey
{
    public class ClientInfoDBContext : DbContext
    {
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlite("Data Source=.\\..\\..\\auction_clients.sqlite");
            string threeLevelsUpPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\");
            optionsBuilder.UseSqlite("Data Source=" + Path.Combine(threeLevelsUpPath, "auction_clients.sqlite"));
        }

        public DbSet<ClientInfo> Clients => Set<ClientInfo>();
    }
}
