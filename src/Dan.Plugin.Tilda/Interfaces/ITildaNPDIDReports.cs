using System;
using System.Threading.Tasks;
using Dan.Common.Models;
using Dan.Plugin.Tilda.Models;

namespace Dan.Plugin.Tilda.Interfaces
{
    public interface ITildaNPDIDAuditReports : ITildaEvidenceType
    {
        public Task<NPDIDAuditReportList> GetNPDIDAuditReportsAsync(EvidenceHarvesterRequest req, DateTime? fromDate, DateTime? toDate, string npdid);
    }
}
