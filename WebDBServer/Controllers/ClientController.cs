using Microsoft.AspNetCore.Mvc;

namespace WebDBServer.Controllers
{
    public class ClientController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
