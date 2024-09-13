using System.Threading.Tasks;
using Dan.Common.Models;
using Dan.Plugin.Tilda.Models;

namespace Dan.Plugin.Tilda.Interfaces
{
    public interface ITildaTrendReportsAll : ITildaSource
    {
        public Task<TrendReportList> GetDataTrendAllAsync(EvidenceHarvesterRequest req, string month, string year, string filter);
    }
}
