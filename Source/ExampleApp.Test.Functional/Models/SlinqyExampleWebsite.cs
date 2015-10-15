using System;

namespace ExampleApp.Test.Functional.Models
{
    public class SlinqyExampleWebsite
    {
        private readonly Uri _exampleWebsiteBaseUri;

        public 
        SlinqyExampleWebsite(
            Uri exampleWebsiteBaseUri)
        {
            _exampleWebsiteBaseUri = exampleWebsiteBaseUri;
        }
    }
}
