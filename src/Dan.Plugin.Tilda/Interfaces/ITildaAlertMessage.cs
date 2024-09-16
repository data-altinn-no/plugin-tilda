using System;
using System.Threading.Tasks;
using Dan.Common.Models;
using Dan.Plugin.Tilda.Models;

namespace Dan.Plugin.Tilda.Interfaces
{
    public interface ITildaAlertMessage : ITildaEvidenceType
    {
        public Task<AlertMessageList> GetAlertMessagesAsync(EvidenceHarvesterRequest req, DateTime? fromDate, DateTime? toDate, string identifier);
    }
}



