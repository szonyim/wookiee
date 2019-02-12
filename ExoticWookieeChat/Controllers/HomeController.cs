using System.Web.Mvc;

namespace ExoticWookieeChat.Controllers
{
    /// <summary>
    /// Welcome page controller
    /// </summary>
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}