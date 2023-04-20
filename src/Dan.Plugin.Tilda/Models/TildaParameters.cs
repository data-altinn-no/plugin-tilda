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

        public Int64? month { get; set; }

        public Int64? year { get; set; }

        public TildaParameters()
        {

        }

        public TildaParameters(DateTime? fromDate, DateTime? toDate, string npdid, bool? includeSubunits, string sourceFilter, string identifier, string filter, Int64? year, Int64? month)
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
        }
    }
}
