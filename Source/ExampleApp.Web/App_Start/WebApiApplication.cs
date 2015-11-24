namespace ExampleApp.Web
{
    using System;
    using System.Web.Http;

    /// <summary>
    /// Configures the WebAPI portion of the framework.
    /// </summary>
    public static class WebApiApplication
    {
        /// <summary>
        /// Configures the specified HttpConfiguration instance.
        /// </summary>
        /// <param name="config">
        /// Specifies the instance to configure.
        /// </param>
        public
        static
        void
        Register(
            HttpConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}