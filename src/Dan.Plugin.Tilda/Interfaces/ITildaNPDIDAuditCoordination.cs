using Dan.Common.Models;
using Dan.Tilda.Models.Audits.Coordination;
using System;
using System.Threading.Tasks;

namespace Dan.Plugin.Tilda.Interfaces
{
    public interface ITildaNPDIDAuditCoordination : ITildaEvidenceType
    {
        public Task<AuditCoordinationList> GetAuditCoordinationAsync(EvidenceHarvesterRequest req, DateTime? fromDate, DateTime? toDate);
    }
}
