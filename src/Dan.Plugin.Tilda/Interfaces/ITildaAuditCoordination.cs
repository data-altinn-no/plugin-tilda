using System;
using System.Threading.Tasks;
using Dan.Common.Models;
using Dan.Plugin.Tilda.Models;

namespace Dan.Plugin.Tilda.Interfaces
{
    public interface ITildaAuditCoordination : ITildaSource
    {
        public Task<AuditCoordinationList> GetAuditCoordinationAsync(EvidenceHarvesterRequest req, DateTime? fromDate, DateTime? toDate);

    }
}
