using Microsoft.AspNetCore.Mvc;
using WebDBServer.Models;
using System.Web;
using Microsoft.AspNetCore.Http;

namespace WebDBServer.Controllers
{
    public class ClientController : Controller
    {
        private readonly Random random = new Random();
        private const int MinPort = 1024; // Minimum valid port number
        private const int MaxPort = 65535; // Maximum valid port number

        public ClientController()
        {
            // Add some sample client data for testing
            ClientListModel.Clients.Add(new ClientModel { IP = "localhost", Port = 1234 });
            ClientListModel.Clients.Add(new ClientModel { IP = "localhost", Port = 5678 });
            // Add more sample clients as needed
        }

        [HttpPost]
        public ActionResult Register()
        {
            int randomPort = GenerateRandomPort();

            ClientModel newClient = new ClientModel { IP = "localhost", Port = randomPort };

            ClientListModel.Clients.Add(newClient);

            //return Json(newClient);
            return Json(new { Message = "Registration Successful" });
        }

        private int GenerateRandomPort()
        {
            return random.Next(MinPort, MaxPort + 1);
        }

        public ActionResult ClientList()
        {
            return Json(ClientListModel.Clients);
        }

        [HttpGet]
        public ActionResult GetOtherClients()
        {
            int currentPort = GetCurrentClientPort();

            List<ClientModel> otherClients = new List<ClientModel>();

            foreach (ClientModel client in ClientListModel.Clients)
            {
                if (client.Port != currentPort)
                {
                    otherClients.Add(client);
                }
            }

            return Json(otherClients);
        }

        private int GetCurrentClientPort()
        {
            int currentPort = ClientListModel.Clients.Last().Port;

            return currentPort;
        }
    }
}