namespace ExampleApp
{
    using System.Net;
    using System.Web.Mvc;

    /// <summary>
    /// Formats unhandled exceptions raised from AJAX controller actions as JSON.  As such, all a Server Exceptions (HTTP 500).
    /// </summary>
    internal sealed class HandleAjaxExceptionAttribute : HandleErrorAttribute
    {
        /// <summary>
        /// Handles exceptions.
        /// </summary>
        /// <param name="filterContext">Specifies the exception context to handle.</param>
        public 
        override 
        void 
        OnException(
            ExceptionContext filterContext)
        {
            if (filterContext != null && filterContext.HttpContext.Request.IsAjaxRequest() && filterContext.Exception != null)
            {
                filterContext.HttpContext.Response.StatusCode = (int) HttpStatusCode.InternalServerError;

                filterContext.Result = new ContentResult
                {
                    Content = filterContext.Exception.Message
                };
                
                filterContext.ExceptionHandled = true;
            }
            else
            {
                base.OnException(filterContext);
            }
        }
    }
}