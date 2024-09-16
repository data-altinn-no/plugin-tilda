using System;
using System.Threading.Tasks;
using Dan.Common.Models;
using Dan.Plugin.Tilda.Models;

namespace Dan.Plugin.Tilda.Interfaces
{
    public interface ITildaAuditReports : ITildaEvidenceType
    {

        public Task<AuditReportList> GetAuditReportsAsync(EvidenceHarvesterRequest req, DateTime? fromDate, DateTime? toDate);
    }
}
