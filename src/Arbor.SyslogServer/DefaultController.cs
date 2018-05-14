using Microsoft.AspNetCore.Mvc;

namespace Arbor.SyslogServer
{
    [Route("/")]
    public class DefaultController : Controller
    {
        [Route("")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
