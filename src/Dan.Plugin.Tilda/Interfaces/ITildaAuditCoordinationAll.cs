using System.Threading.Tasks;
using Dan.Common.Models;
using Dan.Plugin.Tilda.Models;

namespace Dan.Plugin.Tilda.Interfaces
{
    public interface ITildaAuditCoordinationAll : ITildaSource
    {
        public Task<AuditCoordinationList> GetAuditCoordinationAllAsync(EvidenceHarvesterRequest req, string month, string year, string filter);
    }
}
