using Dan.Common.Models;
using Dan.Plugin.Tilda.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dan.Plugin.Tilda.Interfaces
{
    internal interface ITildaPdfReport
    {
        public Task<byte[]> GetPdfReport(EvidenceHarvesterRequest req, string reportIdentifier);
    }
}
