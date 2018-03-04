using System;
using System.Net.Http;
using System.Web.Http.Controllers;
using NUnit.Framework;

namespace HoldYourHorses.Test
{
    [TestFixture]
    public class ThrottleAttributeTest
    {
        private HttpActionContext CreateHttpContext(string key)
        {
            var context = new HttpActionContext();
            var controllerContext = new HttpControllerContext { Request = new HttpRequestMessage() };
            controllerContext.Request.Headers.Add("Authorization", key);
            context.ControllerContext = controllerContext;

            return context;
        }

        [Test]
        public void CallsExceedAllowedCallsInTimeframe()
        {
            var throttle = new ThrottleAttribute() { CallsPerRateLimitSeconds = 1, RateLimitSeconds = 1 };
            var authCode = "Key1" + Guid.NewGuid();
            var context = CreateHttpContext(authCode);
            throttle.OnActionExecuting(context);
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));

            var context2 = CreateHttpContext(authCode);
            throttle.OnActionExecuting(context2);

            Assert.AreEqual(context2.Response.StatusCode.ToString(), 429.ToString());
        }

        [Test]
        public void MultipleCallsFromDifferentUsersDoNotExceed()
        {
            var throttle = new ThrottleAttribute() { CallsPerRateLimitSeconds = 1, RateLimitSeconds = 1 };

            var context = CreateHttpContext("Key3" + Guid.NewGuid());
            throttle.OnActionExecuting(context);

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));

            var context2 = CreateHttpContext("Key4" + Guid.NewGuid());
            throttle.OnActionExecuting(context2);

            Assert.AreEqual(context2.Response, null);
        }

        [Test]
        public void CallsDoNotExceedLimit()
        {
            var throttle = new ThrottleAttribute { CallsPerRateLimitSeconds = 3, RateLimitSeconds = 1 };
            var authCode = "Key2" + Guid.NewGuid();
            var context = CreateHttpContext(authCode);
            throttle.OnActionExecuting(context);

            var context2 = CreateHttpContext(authCode);
            throttle.OnActionExecuting(context2);

            Assert.AreEqual(context2.Response, null);
        }

        [Test]
        public void DefaultsAreSetForThrottling()
        {
            var throttle = new ThrottleAttribute();

            var context = CreateHttpContext("Key" + Guid.NewGuid());
            throttle.OnActionExecuting(context);
            throttle.OnActionExecuting(context);

            Assert.AreEqual(throttle.CallsPerRateLimitSeconds, 10);
            Assert.AreEqual(throttle.RateLimitSeconds, 30);
        }

        [Test]
        public void SuccessfullyMakeCallsAfterThrottlingEnds()
        {
            var throttle = new ThrottleAttribute() { CallsPerRateLimitSeconds = 1, RateLimitSeconds = 1 };
            var authCode = "Key8" + Guid.NewGuid();
            var context = CreateHttpContext(authCode);
            throttle.OnActionExecuting(context);
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));

            var context2 = CreateHttpContext(authCode);
            throttle.OnActionExecuting(context2);

            Assert.AreEqual(context2.Response.StatusCode.ToString(), 429.ToString());


            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));

            var context3 = CreateHttpContext(authCode);
            throttle.OnActionExecuting(context3);

            Assert.AreEqual(context3.Response, null);
        }
    }
}
