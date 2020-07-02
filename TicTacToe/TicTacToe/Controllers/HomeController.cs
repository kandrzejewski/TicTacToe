using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace TicTacToe.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            return await Task.Run(() =>
            {
                var culture = Request.HttpContext.Session.GetString("culture");
                ViewBag.Language = culture;
                return View();
            });
        }

        public async Task<IActionResult> SetCulture(string culture)
        {
            return await Task.Run(() =>
            {
                Request.HttpContext.Session.SetString("culture", culture);
                return RedirectToAction("Index");
            });  
        }
    }
}