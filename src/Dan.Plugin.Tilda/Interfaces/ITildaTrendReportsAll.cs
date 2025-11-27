using System.Threading.Tasks;
using Dan.Common.Models;
using Dan.Plugin.Tilda.Models;
using Dan.Tilda.Models.Audits.Trend;

namespace Dan.Plugin.Tilda.Interfaces
{
    public interface ITildaTrendReportsAll : ITildaEvidenceType
    {
        public Task<TrendReportList> GetDataTrendAllAsync(EvidenceHarvesterRequest req, string month, string year, string filter);
    }
}
