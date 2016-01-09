namespace ExampleApp.Web
{
    using System;
    using System.Web.Optimization;

    /// <summary>
    /// Configures the script bundles for the website.
    /// </summary>
    public static class BundleConfig
    {
        /// <summary>
        /// Applies the website's bundles to the specified collection.
        /// </summary>
        /// <param name="bundles">Specifies the instance to configure.</param>
        /// <exception cref="ArgumentNullException">Thrown if the bundles parameter value is null.</exception>
        public
        static
        void
        RegisterBundles(
            BundleCollection bundles)
        {
            if (bundles == null)
                throw new ArgumentNullException(nameof(bundles));

            var jqueryBundle = new ScriptBundle("~/bundles/jquery");

            jqueryBundle.Include("~/Scripts/jquery-{version}.js");
            jqueryBundle.Include("~/Scripts/jquery.validate.js");
            jqueryBundle.Include("~/Scripts/jquery.validate.unobtrusive.js");
            jqueryBundle.Include("~/Scripts/jquery.unobtrusive-ajax.js");

            bundles.Add(jqueryBundle);
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include("~/Scripts/modernizr-*"));
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include("~/Scripts/bootstrap.js", "~/Scripts/respond.js"));
            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/bootstrap.css", "~/Content/site.css", "~/Content/ajax-indicator.css"));
            bundles.Add(new ScriptBundle("~/bundles/slinqy").Include("~/Scripts/ajax.indicator.js"));
        }
    }
}