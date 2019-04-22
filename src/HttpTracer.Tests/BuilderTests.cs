using System;
using System.Net.Http;
using HttpTracer.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HttpTracer.Tests
{
    [TestClass]
    public class BuilderTests
    {
        [TestMethod]
        public void MakeSureHierarchyIsBuilt()
        {
            var builder = new HttpHandlerBuilder();
            builder.AddHandler(new FakeHandler())
                .AddHandler(new SillyHandler())
                .AddHandler(new FakeHandler());

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
            builder.AddHandler(new FakeHandler())
                .AddHandler(new SillyHandler())
                .AddHandler(new FakeHandler());

            var first = builder.Build();
            var second = ((DelegatingHandler)first).InnerHandler;
            var third = ((DelegatingHandler)second).InnerHandler;
            var fourth = ((DelegatingHandler)third).InnerHandler;

            Assert.IsInstanceOfType(first, typeof(FakeHandler));
            Assert.IsInstanceOfType(second, typeof(SillyHandler));
            Assert.IsInstanceOfType(third, typeof(FakeHandler));
            Assert.IsInstanceOfType(fourth, typeof(HttpTracerHandler));
        }

        [TestMethod]
        public void MakeSureOnlyCallingBuildWillReturnOurHanlder()
        {
            var ourHandler = new HttpHandlerBuilder().Build();
            
            Assert.IsInstanceOfType(ourHandler, typeof(HttpTracerHandler));
        }

        [TestMethod]
        public void MakingSureTheInnerHandlerOfOurHandlerIsNotNull()
        {
            var ourHandler = new HttpHandlerBuilder().Build();

            var inner = ((DelegatingHandler)ourHandler).InnerHandler;

            Assert.IsInstanceOfType(inner, typeof(HttpMessageHandler));
        }

        [TestMethod]
        public void AddingHttpMessageHandlerToAddHandlerMethodTHrowsArgumentException()
        {
            try
            {
                var ourHandler = new HttpHandlerBuilder().AddHandler(new HttpTracerHandler());
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentException));
                Assert.AreEqual(ex.Message, $"Can't add handler of type {nameof(HttpTracerHandler)}.");
            }
        }
    }
}
