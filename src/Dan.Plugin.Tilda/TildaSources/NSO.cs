using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Models;
using Dan.Plugin.Tilda.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dan.Plugin.Tilda.TildaSources
{
    public class NSO : TildaDataSource
    {
        private const string orgNo = "960478150";
        public const string controlAgency = "Næringslivets sikkerhetsorganisasjon";

        public override string ControlAgency
        {
            get => controlAgency;
        }

        public override string OrganizationNumber
        {
            get => orgNo;
        }

        public NSO(Settings settings, HttpClient client, ILogger logger) : base(settings,
            client, logger)
        {

        }

        public NSO() : base()
        {

        }
    }
}
