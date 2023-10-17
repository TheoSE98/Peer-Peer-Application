using Microsoft.AspNetCore.Mvc;
using WebDBServer.Models;
using System.Web;
using Microsoft.AspNetCore.Http;

namespace WebDBServer.Controllers
{
    public class ClientController : Controller
    {
        //private readonly Random random = new Random();
        //private const int MinPort = 1024; // Minimum valid port number
        //private const int MaxPort = 65535; // Maximum valid port number

        public ClientController()
        {
            // Testing Data 
            ClientListModel.Clients.Add(new ClientModel { IP = "localhost", Port = 1234 });
            ClientListModel.Clients.Add(new ClientModel { IP = "localhost", Port = 5678 });
            ClientListModel.Clients.Add(new ClientModel { IP = "localhost", Port = 4321 });
            ClientListModel.Clients.Add(new ClientModel { IP = "localhost", Port = 8765 });

        }

        [HttpPost]
        public ActionResult Register([FromBody] ClientModel newClient)
        {
            // Add the client to the static list
            ClientListModel.Clients.Add(newClient);

            return Json(new { Message = "Registration Successful" });
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