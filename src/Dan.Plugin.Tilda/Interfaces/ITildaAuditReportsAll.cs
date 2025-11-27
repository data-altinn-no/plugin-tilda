using System.Threading.Tasks;
using Dan.Common.Models;
using Dan.Plugin.Tilda.Models;
using Dan.Tilda.Models.Audits.Report;

namespace Dan.Plugin.Tilda.Interfaces
{
    public interface ITildaAuditReportsAll : ITildaEvidenceType
    {

        public Task<AuditReportList> GetAuditReportsAllAsync(EvidenceHarvesterRequest req, string month, string year, string filter);
    }
}
