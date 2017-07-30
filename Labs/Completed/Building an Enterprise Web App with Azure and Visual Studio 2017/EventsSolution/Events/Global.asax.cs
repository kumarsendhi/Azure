using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using System.Configuration;
using StackExchange.Redis;


namespace Events
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static ConnectionMultiplexer redisCache;
        public static ConnectionMultiplexer RedisCache
        {
            get
            {
                if (redisCache == null || !redisCache.IsConnected)
                {
                    redisCache = ConnectionMultiplexer.Connect(ConfigurationManager.ConnectionStrings["RedisConnection"].ConnectionString);
                }
                return redisCache;
            }
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            var strategy = new FixedInterval("fixed", 10, TimeSpan.FromSeconds(3));
            var strategies = new List<RetryStrategy> { strategy };
            var manager = new RetryManager(strategies, "fixed");
            RetryManager.SetDefault(manager);
        }
    }
}
