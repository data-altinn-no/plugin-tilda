using Nadobe.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dan.Common.Models;
using Dan.Plugin.Tilda.Models;

namespace Dan.Plugin.Tilda.Interfaces
{
    interface ITildaAlertMessage
    {
        public Task<AlertMessageList> GetAlertMessagesAsync(EvidenceHarvesterRequest req, DateTime? fromDate, DateTime? toDate, string identifier);
    }
}



