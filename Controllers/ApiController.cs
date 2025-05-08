using Microsoft.AspNetCore.Mvc;

namespace OJTMAjax.Controllers
{
    public class ApiController : Controller
    {
        public IActionResult Index()
        {
            return Content("Ajax 好!!","text/plain", System.Text.Encoding.UTF8);
        }
    }
}
