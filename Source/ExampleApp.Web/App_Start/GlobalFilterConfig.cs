namespace ExampleApp.Web
{
    using System.Web.Mvc;
    using Filters;

    /// <summary>
    /// Configures the GlobalFilterCollection.
    /// </summary>
    internal static class GlobalFilterConfig
    {
        /// <summary>
        /// Applies application specific filters to the specified collection.
        /// </summary>
        /// <param name="filters">Specifies the collection to add filters to.</param>
        public
        static
        void
        RegisterGlobalFilters(
            GlobalFilterCollection filters)
        {
            filters.Add(new HandleAjaxExceptionAttribute());
        }
    }
}