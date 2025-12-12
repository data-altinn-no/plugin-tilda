using System;
using System.Threading.Tasks;
using Dan.Common.Models;
using Dan.Plugin.Tilda.Models;
using Dan.Tilda.Models.Audits.Trend;

namespace Dan.Plugin.Tilda.Interfaces
{
    public interface ITildaTrendReports : ITildaEvidenceType
    {
        public Task<TrendReportList> GetDataTrendAsync(EvidenceHarvesterRequest req, DateTime? fromDate, DateTime? toDate);
    }
}
