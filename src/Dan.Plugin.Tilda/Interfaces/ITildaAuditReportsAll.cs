using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dan.Common.Models;
using Dan.Plugin.Tilda.Models;
using Nadobe.Common.Models;

namespace Dan.Plugin.Tilda.Interfaces
{
    public interface ITildaAuditReportsAll
    {

        public Task<AuditReportList> GetAuditReportsAllAsync(EvidenceHarvesterRequest req, DateTime? fromDate, DateTime? toDate, string filter);
    }
}
