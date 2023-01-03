using System;

namespace Dan.Plugin.Tilda.Config
{
    public class Settings
    {
        public string GetClassBaseUri(string className)
        {
            return Environment.GetEnvironmentVariable(className + ".uri");
        }

        public string RedisConnectionString { get; set; }
       
        public bool IsTest { get; set; }

        public bool IsLocalDevelopment { get; set; }

        public string Breaker_RetryWaitTime { get; set; }
        public string Breaker_OpenCircuitTime { get; set; }
    }
}
