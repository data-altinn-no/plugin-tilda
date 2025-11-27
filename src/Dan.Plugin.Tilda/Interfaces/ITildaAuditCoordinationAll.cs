using System.Threading.Tasks;
using Dan.Common.Models;
using Dan.Plugin.Tilda.Models;
using Dan.Tilda.Models.Audits.Coordination;

namespace Dan.Plugin.Tilda.Interfaces
{
    public interface ITildaAuditCoordinationAll : ITildaEvidenceType
    {
        public Task<AuditCoordinationList> GetAuditCoordinationAllAsync(EvidenceHarvesterRequest req, string month, string year, string filter);
    }
}
