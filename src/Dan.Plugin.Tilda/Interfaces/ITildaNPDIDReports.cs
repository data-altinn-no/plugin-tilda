using System;
using System.Threading.Tasks;
using Dan.Common.Models;
using Dan.Plugin.Tilda.Models;
using Dan.Tilda.Models.Audits.NPDID;

namespace Dan.Plugin.Tilda.Interfaces
{
    public interface ITildaNPDIDAuditReports : ITildaEvidenceType
    {
        public Task<NpdidAuditReportList> GetNPDIDAuditReportsAsync(EvidenceHarvesterRequest req, DateTime? fromDate, DateTime? toDate, string npdid);
    }
}
