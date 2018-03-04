using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Caching;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace HoldYourHorses.ActionFilters
{
    public class ThrottleAttribute : ActionFilterAttribute
    {
        public int RateLimitSeconds { get; set; }
        public int CallsPerRateLimitSeconds { get; set; }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            actionContext.Request.Headers.TryGetValues("Authorization", out var keys);

            var key = keys.FirstOrDefault();
            if (key == null) return;

            if (IsRateLimited(key))
            {
                SetRateLimitExceededMessage(actionContext);
            }
        }

        private void SetRateLimitExceededMessage(HttpActionContext actionContext)
        {
            actionContext.Response = new HttpResponseMessage
            {
                StatusCode = (HttpStatusCode)429,
                ReasonPhrase = "Too Many Requests",
                Content = new StringContent($"Rate limit reached {CallsPerRateLimitSeconds} calls in {RateLimitSeconds} seconds.")
            };
        }

        private bool IsRateLimited(string key)
        {

            var callRateForAuthorizationCode = (CallRateForAuthorizationCode)HttpRuntime.Cache.Get(key);

            SetDefaultRateLimit();

            if (callRateForAuthorizationCode != null)
            {

                if (TooManyCallsInRateLimitTime(callRateForAuthorizationCode)) return true;

                callRateForAuthorizationCode.Calls++;
                SetCacheValue(key, callRateForAuthorizationCode);
            }
            else
            {
                SetCacheValue(key, new CallRateForAuthorizationCode { Calls = 1, FirstCallTime = DateTime.Now });
            }

            return false;
        }

        private bool TooManyCallsInRateLimitTime(CallRateForAuthorizationCode callRateForAuthorizationCode)
        {
            var secondsPassedSinceFirstRecordedCall = DateTime.Now.Subtract(callRateForAuthorizationCode.FirstCallTime).Seconds;
            return callRateForAuthorizationCode.Calls >= CallsPerRateLimitSeconds && secondsPassedSinceFirstRecordedCall <= RateLimitSeconds;
        }

        private void SetDefaultRateLimit()
        {
            if (RateLimitSeconds == 0)
            {
                RateLimitSeconds = 30;
            }

            if (CallsPerRateLimitSeconds == 0)
            {
                CallsPerRateLimitSeconds = 10;
            }
        }


        private void SetCacheValue(string key, CallRateForAuthorizationCode callRateForAuthorizationCode)
        {
            HttpRuntime.Cache.Insert(key,
                callRateForAuthorizationCode,
                null,
                DateTime.Now.AddSeconds(RateLimitSeconds * 2),
                Cache.NoSlidingExpiration,
                CacheItemPriority.Low,
                null);
        }
    }

    public class CallRateForAuthorizationCode
    {
        public int Calls { get; set; }

        public DateTime FirstCallTime { get; set; }
    }
}
