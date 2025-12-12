using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Dan.Common.Models;
using Dan.Tilda.Models.Alerts;

namespace Dan.Plugin.Tilda.Interfaces
{
    public interface ITildaAlertMessage : ITildaEvidenceType
    {
        public Task<AlertSourceMessage> GetAlertMessageAsync(EvidenceHarvesterRequest req, string identifier);
        public Task<List<AlertSourceMessage>> GetAlertMessagesAsync(string from);
        public Task SendAlertMessageAsync(CloudEvent cloudEvent);
    }
}



