using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dan.Common.Models;
using Dan.Plugin.Tilda.Models;
using Nadobe.Common.Models;

namespace Dan.Plugin.Tilda.Interfaces
{
    public interface ITildaAuditCoordinationAll
    {
        public Task<AuditCoordinationList> GetAuditCoordinationAllAsync(EvidenceHarvesterRequest req, string month, string year, string filter);
    }
}
