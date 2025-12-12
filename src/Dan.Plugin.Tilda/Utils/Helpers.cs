using Dan.Plugin.Tilda.Interfaces;
using Dan.Plugin.Tilda.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using Dan.Plugin.Tilda.Extensions;
using Dan.Tilda.Models.Audits.Coordination;
using Dan.Tilda.Models.Audits.NPDID;
using Dan.Tilda.Models.Audits.Report;
using Dan.Tilda.Models.Audits.Trend;
using Dan.Tilda.Models.Enums;


namespace Dan.Plugin.Tilda.Utils
{
    public static class Helpers
    {
        public static AuditReportList GetEmptyResponseAuditReportList(string controlAgency)
        {
            return new AuditReportList(controlAgency)
            {
                StatusText = $"Tomt resultat fra {controlAgency}",
                Status = StatusEnum.NotFound
            };
        }

        public static AuditReportList GetEmptyFailedResponseAuditReportList(string controlAgency)
        {
            return new AuditReportList(controlAgency)
            {
                StatusText = $"Kunne ikke hente data fra {controlAgency}, ",
                Status = StatusEnum.Failed
            };
        }

        public static NpdidAuditReportList GetEmptyResponseNPDIDAuditReportList(string controlAgency)
        {
            return new NpdidAuditReportList(controlAgency)
            {
                StatusText = $"Tomt resultat fra {controlAgency}",
                Status = StatusEnum.NotFound
            };
        }

        public static NpdidAuditReportList GetEmptyFailedResponseNPDIDAuditReportList(string controlAgency)
        {
            return new NpdidAuditReportList(controlAgency)
            {
                StatusText = $"Kunne ikke hente data fra {controlAgency}, ",
                Status = StatusEnum.Failed
            };
        }

        public static TrendReportList GetEmptyResponseTrendReportList(string controlAgency)
        {
            return new TrendReportList(controlAgency)
            {
                StatusText = $"Tomt resultat fra {controlAgency}",
                Status = StatusEnum.NotFound
            };
        }

        public static TrendReportList GetEmptyFailedResponseTrendReportList(string controlAgency)
        {
            return new TrendReportList(controlAgency)
            {
                StatusText = $"Kunne ikke hente data fra {controlAgency}, ",
                Status = StatusEnum.Failed
            };
        }

        public static AuditCoordinationList GetEmptyResponseCoordinationList(string controlAgency)
        {
            return new AuditCoordinationList(controlAgency)
            {
                StatusText = $"Tomt resultat fra {controlAgency}",
                Status = StatusEnum.NotFound
            };
        }

        public static AuditCoordinationList GetEmptyFailedResponseCoordinationList(string controlAgency,
            string statusText = "")
        {
            return new AuditCoordinationList(controlAgency)
            {
                StatusText = statusText,
                Status = StatusEnum.Failed
            };
        }
    }
}
