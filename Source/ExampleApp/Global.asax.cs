using System.Web.Mvc;
using System.Web.Routing;

namespace ExampleApp
{
    /// <summary>
    /// Handles application life cycle events.
    /// </summary>
    public class WebApiApplication : System.Web.HttpApplication
    {
        /// <summary>
        /// This method is automatically called when the application first starts.
        /// </summary>
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}
