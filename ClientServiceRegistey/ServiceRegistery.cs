using BF.IY.Common.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientServiceRegistey
{
    public static class ServiceRegistery
    {
        public static readonly ClientInfoDBContext dbContext = new ClientInfoDBContext();

        static ServiceRegistery()
        {   
            dbContext.Database.EnsureCreated();
        }


        public static void CleanServiceRegistery()
        {
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
        }
        public static bool RegisterClient(ClientInfo client)
        {
            bool isAdded = false;
            try
            {
               var addedClient = dbContext.Clients.Add(client);
                dbContext.SaveChanges();
                isAdded = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return isAdded;
        }

        public static ClientInfo GetClient(int clientId)
        {
            ClientInfo client = null;
            try
            {
                client = dbContext.Clients.AsNoTracking().Where(c => c.Id == clientId).FirstOrDefault()!;
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return client;
        }

        public static List<ClientInfo> GetAllRegisteredClients()
        {
            List<ClientInfo> clients = null;
            try
            {
                clients = dbContext.Clients.AsNoTracking().ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return clients;
        }
    }
}
