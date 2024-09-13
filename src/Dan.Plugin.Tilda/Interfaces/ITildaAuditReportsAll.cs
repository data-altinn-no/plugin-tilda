using System.Threading.Tasks;
using Dan.Common.Models;
using Dan.Plugin.Tilda.Models;

namespace Dan.Plugin.Tilda.Interfaces
{
    public interface ITildaAuditReportsAll : ITildaSource
    {

        public Task<AuditReportList> GetAuditReportsAllAsync(EvidenceHarvesterRequest req, string month, string year, string filter);
    }
}
