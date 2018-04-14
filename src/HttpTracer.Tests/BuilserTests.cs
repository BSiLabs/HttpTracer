using System;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HttpTracer.Tests
{
    public class MyHandler1 : DelegatingHandler
    {
        public MyHandler1()
        {
            InnerHandler = new HttpClientHandler();
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            await Task.Delay(1, cancellationToken);

            request.Headers.Add("SILLY-HEADER", "SILLY VALUE");

            Debug.WriteLine("HI I'M MyHandler1");

            await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            return new HttpResponseMessage();

        }
    }

    public class MyHandler3 : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            await Task.Delay(1, cancellationToken);
            request.Headers.Add("SILLY-HEADER-3", "SILLY VALUE 3");

            await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            return new HttpResponseMessage();
        }
    }

    [TestClass]
    public class BuilserTests
    {
        [TestMethod]
        public void MakeSureHierarchyIsBuilt()
        {
            var builder = new HttpHandlerBuilder();
            builder.AddHttpHandlers(new MyHandler3())
                .AddHttpHandlers(new MyHandler1())
                .AddHttpHandlers(new MyHandler3());

            var httpHandler = builder.Build();
            var first = ((DelegatingHandler)httpHandler).InnerHandler;
            var second = ((DelegatingHandler)first).InnerHandler;
            var third = ((DelegatingHandler)second).InnerHandler;
            var fourth = ((DelegatingHandler)third).InnerHandler;

            Assert.IsNotNull(first);
            Assert.IsNotNull(second);
            Assert.IsNotNull(third);
            Assert.IsNotNull(fourth);
        }

        [TestMethod]
        public void MakeSureHierarchyIsBuiltWithCorrectType()
        {
            var builder = new HttpHandlerBuilder();
            builder.AddHttpHandlers(new MyHandler3())
                .AddHttpHandlers(new MyHandler1())
                .AddHttpHandlers(new MyHandler3());

            var first = builder.Build();
            var second = ((DelegatingHandler)first).InnerHandler;
            var third = ((DelegatingHandler)second).InnerHandler;
            var fourth = ((DelegatingHandler)third).InnerHandler;

            Assert.IsInstanceOfType(first, typeof(MyHandler3));
            Assert.IsInstanceOfType(second, typeof(MyHandler1));
            Assert.IsInstanceOfType(third, typeof(MyHandler3));
            Assert.IsInstanceOfType(fourth, typeof(HttpTracerHandler));
        }

        [TestMethod]
        public void MakeSureOnlyCallingBuildWillReturnOurHanlder()
        {
            var ourHandler = new HttpHandlerBuilder().Build();
            
            Assert.IsInstanceOfType(ourHandler, typeof(HttpTracerHandler));
        }

        [TestMethod]
        public void MakingSureTheInnerHandlerOfOurHandlerIsNitNull()
        {
            var ourHandler = new HttpHandlerBuilder().Build();

            var inner = ((DelegatingHandler)ourHandler).InnerHandler;

            Assert.IsInstanceOfType(inner, typeof(HttpMessageHandler));
        }
    }
}
