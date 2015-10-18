namespace ExampleApp
{
    using System.Web.Mvc;
    using System.Web.Routing;

    /// <summary>
    /// Defines the HTTP routes for the application.
    /// </summary>
    public static class RouteConfig
    {
        /// <summary>
        /// Applies routes to the specified RouteCollection.
        /// </summary>
        /// <param name="routes">Specifies the RouteCollection to configure.</param>
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional });
        }
    }
}
