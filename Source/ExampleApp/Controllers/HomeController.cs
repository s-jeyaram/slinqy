using System.Web.Mvc;

namespace ExampleApp.Controllers
{
    /// <summary>
    /// Defines supported actions for the Homepage.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Handles the default HTTP GET /.
        /// </summary>
        /// <returns>Returns the default Homepage view.</returns>
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }
    }
}
