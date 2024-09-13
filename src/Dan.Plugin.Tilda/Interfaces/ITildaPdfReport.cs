using Dan.Common.Models;
using System.Threading.Tasks;

namespace Dan.Plugin.Tilda.Interfaces
{
    internal interface ITildaPdfReport : ITildaSource
    {
        public Task<byte[]> GetPdfReport(EvidenceHarvesterRequest req, string reportIdentifier);
    }
}
