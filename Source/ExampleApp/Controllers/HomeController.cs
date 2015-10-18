namespace ExampleApp.Controllers
{
    using System.Web.Mvc;

    /// <summary>
    /// Defines supported actions for the Homepage.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Handles the default HTTP GET /.
        /// </summary>
        /// <returns>Returns the default Homepage view.</returns>
        public 
        ActionResult 
        Index()
        {
            this.ViewBag.Title = "Home Page";

            return this.View();
        }
    }
}
