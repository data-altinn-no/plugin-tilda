using System;
using System.Threading.Tasks;
using Dan.Common.Models;
using Dan.Plugin.Tilda.Models;

namespace Dan.Plugin.Tilda.Interfaces
{
    public interface ITildaTrendReports : ITildaSource
    {
        public Task<TrendReportList> GetDataTrendAsync(EvidenceHarvesterRequest req, DateTime? fromDate, DateTime? toDate);
    }
}
