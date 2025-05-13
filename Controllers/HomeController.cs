using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OJTMAjax.Models;

namespace OJTMAjax.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        //����Promise�Baysnc await
        public IActionResult CallBackSolve()
        {
            return View();
        }

        //�Ʋ�JSON���Ū�����y�k
        //������
        public IActionResult JsonDemo()
        {
            return View();
        }

        public IActionResult Travel()
        {
            return View();
        }

        public IActionResult FirstAjax()
        {     
            return View();
        }

        public IActionResult StopAjax()
        {
            return View();
        }

        public IActionResult Address()
        {
            return View();
        }

        public IActionResult ShowImage() {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult Spots()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
