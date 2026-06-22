using Dan.Common.Models;
using Dan.Tilda.Models.Audits.NPDID;
using Dan.Tilda.Models.Audits.Report;
using System;
using System.Threading.Tasks;

namespace Dan.Plugin.Tilda.Interfaces
{
    public interface ITildaNPDIDAuditReports : ITildaEvidenceType
    {
        public Task<NpdidAuditReportList> GetNPDIDAuditReportsAsync(EvidenceHarvesterRequest req, DateTime? fromDate, DateTime? toDate);
        public Task<AuditReportList> GetAuditReportsAsync(EvidenceHarvesterRequest req, DateTime? fromDate, DateTime? toDate);
    }
}
