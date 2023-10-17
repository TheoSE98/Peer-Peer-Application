using Microsoft.AspNetCore.Mvc;
using WebDBServer.Models;
using System.Web;
using Microsoft.AspNetCore.Http;

namespace WebDBServer.Controllers
{
    public class ClientController : Controller
    {
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