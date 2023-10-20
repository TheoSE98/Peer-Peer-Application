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
            if (newClient == null)
            {
                return BadRequest("Invalid client data.");
            }

            // Check if the client is already registered based on their port
            if (ClientListModel.Clients.Any(c => c.Port == newClient.Port))
            {
                return Conflict("Client with the same port is already registered.");
            }

            // Add the client to the static list
            ClientListModel.Clients.Add(newClient);

            return Json(new { Message = "Registration Successful" });
        }

        public ActionResult ClientList()
        {
            List<ClientModel> clients = ClientListModel.Clients;
            if (clients.Count == 0)
            {
                return NotFound("No clients are registered.");
            }

            return Json(clients);
        }

        [HttpGet]
        public ActionResult GetOtherClients()
        {
            int currentPort = GetCurrentClientPort();

            if (currentPort == -1)
            {
                return NotFound("No clients are registered.");
            }

            List<ClientModel> otherClients = ClientListModel.Clients
                .Where((ClientModel client) => client.Port != currentPort)
                .ToList();

            if (otherClients.Count == 0)
            {
                return NotFound("No other clients found.");
            }

            return Json(otherClients);
        }

        private int GetCurrentClientPort()
        {
            ClientModel lastClient = ClientListModel.Clients.LastOrDefault();
            if (lastClient == null)
            {
                return -1;
            }

            return lastClient.Port;
        }

        [HttpPost]
        public ActionResult PostJobResult([FromBody] JobResultModel jobResult)
        {
            if (jobResult == null)
            {
                return BadRequest("Invalid job result data");
            }

            JobResultListModel.JobResults.Add(jobResult);

            return Json(new { Message = "Job result posted successfully" });
        }
    }
}