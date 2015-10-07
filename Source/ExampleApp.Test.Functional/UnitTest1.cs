using System;
using System.Diagnostics;
using Xunit;

namespace ExampleApp.Test.Functional
{
    public class UnitTest1
    {
        [Fact]
        public void TestMethod1()
        {
            Console.WriteLine("CONSOLE: TestMethod1");
            Trace.TraceError("TRACE: ERROR!");
        }
    }
}
