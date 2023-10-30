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

        [HttpGet]
        public ActionResult GetJobResults()
        {
            // Retrieve and return the list of job results
            List<JobResultModel> jobResults = JobResultListModel.JobResults;

            if (jobResults.Count == 0)
            {
                return NotFound("No job results available.");
            }

            return Json(jobResults);
        }
    }
}