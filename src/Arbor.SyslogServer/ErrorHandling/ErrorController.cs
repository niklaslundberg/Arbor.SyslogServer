using Microsoft.AspNetCore.Mvc;

namespace Arbor.SyslogServer.ErrorHandling
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : Controller
    {
        [HttpGet]
        [Route("~/home/error")]
        [Route("~/error")]
        public IActionResult Error(string errorId)
        {
            var vm = new ErrorViewModel();

            return View("Error", vm);
        }
    }
}
