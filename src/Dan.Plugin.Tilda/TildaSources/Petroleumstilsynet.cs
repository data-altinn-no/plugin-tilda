using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Dan.Plugin.Tilda.Config;
using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Models;
using Microsoft.Extensions.Logging;

namespace Dan.Plugin.Tilda.TildaSources
{
    public class Petroleumstilsynet : TildaDataSource, ITildaNPDIDAuditReports
    {
        private const string orgNo = "986174613";
        public const string controlAgency = "Petroleumstilsynet";

        public override string ControlAgency
        {
            get => controlAgency;
        }

        public override string OrganizationNumber
        {
            get => orgNo;
        }

        public Petroleumstilsynet(Settings settings, HttpClient client, ILogger logger) : base(settings,
            client, logger)
        {

        }

        public Petroleumstilsynet() : base()
        {

        }
    }
}
