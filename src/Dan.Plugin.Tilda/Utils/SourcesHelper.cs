using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Models;
using Dan.Plugin.Tilda.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dan.Plugin.Tilda.Utils
{
    public static class SourcesHelper
    { 
        public static IEnumerable<TildaDataSource> GetAllSources<T>(Settings settings, HttpClient client, ILogger logger, bool createInstance = true)
        {
            try
            {
                var a = new List<TildaDataSource>();

                var classes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(t => t.GetTypes())
                    .Where(t => t.IsClass && t.Namespace == "ES_AGGREGATE_AUDIT_V3.TildaSources" &&
                                t.IsNestedPrivate == false && t.GetInterfaces().Contains(typeof(T)));

             
                    foreach (Type cls in classes)
                    {
                        var test = Activator.CreateInstance(cls, settings, client, logger);
                        if (!(test as TildaDataSource).TestOnly || settings.IsTest)
                            a.Add((TildaDataSource)test);
                    }

                    return a;

            }
            catch (Exception ass)
            {
                logger.LogError(ass.Message);
                return null;
            }
        }

        public static IEnumerable<TildaDataSource> GetRelevantSources<T>(string sourceFilter, HttpClient client, ILogger logger, Settings settings)
        {
            try
            {
                var a = new List<TildaDataSource>();

                var classes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(t => t.GetTypes())
                    .Where(t => t.IsClass && t.Namespace == "ES_AGGREGATE_AUDIT_V3.TildaSources" &&
                                t.IsNestedPrivate == false && t.GetInterfaces().Contains(typeof(T)));

                foreach (Type cls in classes)
                {
                    var test = Activator.CreateInstance(cls, settings, client, logger);

                    if ((sourceFilter == null || sourceFilter.Contains((test as TildaDataSource).OrganizationNumber)) && (!(test as TildaDataSource).TestOnly || settings.IsTest))
                        a.Add((TildaDataSource)test);
                }

                return a;
            }
            catch (Exception ass)
            {
                logger.LogError(ass.Message);
                return null;
            }
        }

        public static IEnumerable<TildaDataSource> GetAllRegisteredSources(Settings settings)
        {
            try
            {
                var a = new List<TildaDataSource>();

                var classes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(t => t.GetTypes())
                    .Where(t => t.IsClass && t.Namespace == "ES_AGGREGATE_AUDIT_V3.TildaSources" && t.IsNestedPrivate == false);

                foreach (Type cls in classes)
                {
                    var test = Activator.CreateInstance(cls);
                    if (!(test as TildaDataSource).TestOnly || settings.IsTest)
                        a.Add((TildaDataSource)test);
                }

                return a;
            }
            catch (Exception)
            {
                return new List<TildaDataSource>();
            }
        }
    }
}
