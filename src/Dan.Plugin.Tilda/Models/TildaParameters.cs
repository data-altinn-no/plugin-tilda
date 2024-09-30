using System;
using System.Collections.Generic;
using System.Text;

namespace Dan.Plugin.Tilda.Models
{
    public class TildaParameters
    {
        public DateTime? fromDate { get; set; }
        public DateTime? toDate { get; set; }
        public string npdid { get; set; }
        public bool? includeSubunits { get; set; }
        public string sourceFilter { get; set; }

        public string identifier { get; set; }

        public string filter { get; set; }

        public string month { get; set; }

        public string year { get; set; }

        public string postcode { get; set; }

        public string municipalityNumber { get; set; }

        public string nace { get; set; }

        public TildaParameters(DateTime? fromDate, DateTime? toDate, string npdid, bool? includeSubunits, string sourceFilter, string identifier, string filter, string year, string month, string postcode, string municipalityNumber, string nace)
        {
            this.fromDate = fromDate;
            this.toDate = toDate;
            this.npdid = npdid;
            this.includeSubunits = includeSubunits;
            this.sourceFilter = sourceFilter;
            this.identifier = identifier;
            this.filter = filter;
            this.year = year;
            this.month = month;
            this.postcode = postcode;
            this.municipalityNumber = municipalityNumber;
            this.nace = nace;
        }
    }
}
