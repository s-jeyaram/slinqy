namespace ExampleApp.Web
{
    using System.Diagnostics.CodeAnalysis;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;

    /// <summary>
    /// Handles application life cycle events.
    /// </summary>
    public class WebApiApplication : HttpApplication
    {
        /// <summary>
        /// This method is automatically called when the application first starts.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "ASP.NET will not call this method if it is static.")]
        protected 
        void 
        Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            GlobalFilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
        }
    }
}
