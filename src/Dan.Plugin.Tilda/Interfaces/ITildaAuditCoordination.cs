using System;
using System.Threading.Tasks;
using Dan.Common.Models;
using Dan.Plugin.Tilda.Models;
using Dan.Tilda.Models.Audits.Coordination;

namespace Dan.Plugin.Tilda.Interfaces
{
    public interface ITildaAuditCoordination : ITildaEvidenceType
    {
        public Task<AuditCoordinationList> GetAuditCoordinationAsync(EvidenceHarvesterRequest req, DateTime? fromDate, DateTime? toDate);

    }
}
