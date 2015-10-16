using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ExampleApp.Test.Functional.Models
{
    public class WebBrowser : IDisposable
    {
        private const string WebPageRelativePathConstantName = "RelativePath";

        private static readonly Dictionary<string, Type> WellKnownPages;

        private readonly IWebDriver _webBrowserDriver;
        private readonly Uri        _baseUri;

        static WebBrowser()
        {
            WellKnownPages = GetWellKnownPages();
        }

        public 
        WebBrowser(Uri baseUri)
        {
            _baseUri          = baseUri;
            _webBrowserDriver = new ChromeDriver(); // TODO: Inject
        }

        static 
        Dictionary<string, Type> 
        GetWellKnownPages()
        {
            var types = Assembly
                .GetExecutingAssembly()
                .GetTypes();

            var wellKnownPages = new Dictionary<string, Type>();

            foreach (var type in types.Where(t => typeof(WebPage).IsAssignableFrom(t) && !t.IsAbstract))
            {
                var relativePathField = type
                    .GetFields(BindingFlags.Public | BindingFlags.Static)
                    .SingleOrDefault(fi => 
                        fi.IsLiteral && 
                        !fi.IsInitOnly && 
                        fi.Name == WebPageRelativePathConstantName
                    );

                if (relativePathField == null)
                    throw new InvalidOperationException(
                        $"You must add a public string constant named {WebPageRelativePathConstantName} to type {type.FullName} before it can be used."
                    );

                wellKnownPages.Add(
                    relativePathField.GetRawConstantValue().ToString(), 
                    type
                );
            }

            return wellKnownPages;
        }

        public 
        TPage NavigateTo<TPage>() where TPage: WebPage
        {
            var relativeUri = WellKnownPages.Single(pair => pair.Value == typeof (TPage)).Key;

            var fullyQualifiedUri = new Uri(
                _baseUri,
                relativeUri
            );

            _webBrowserDriver
                .Navigate()
                .GoToUrl(fullyQualifiedUri);

            return GetCurrentPageAs<TPage>();
        }

        public TPage GetCurrentPageAs<TPage>() where TPage : class
        {
            var uri = new Uri(_webBrowserDriver.Url);
            var path = uri.AbsolutePath;

            var match = WellKnownPages.FirstOrDefault(kvp => kvp.Key == path);

            if (match.Key == null)
                throw new Exception("Could not find a matching page for path " + path);

            var pageType = match.Value;

            return (TPage)Activator.CreateInstance(pageType, _webBrowserDriver);
        }
        
        public void Dispose()
        {
            _webBrowserDriver.Dispose();
        }
    }
}