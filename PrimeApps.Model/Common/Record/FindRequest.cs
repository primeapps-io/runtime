using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PrimeApps.Model.Common.Annotations;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Common.Record
{
    public class FindRequest
    {
        public List<string> Fields { get; set; }

        public List<Filter> Filters { get; set; }

        [StringLength(50), BalancedParentheses, FilterLogic]
        public string FilterLogic { get; set; }

        public string SortField { get; set; }

        public SortDirection SortDirection { get; set; }

        public int Limit { get; set; }

        public int Offset { get; set; }

        public string ManyToMany { get; set; }

        public bool TwoWay { get; set; }

        public LogicType LogicType { get; set; }

        public string GroupBy { get; set; }

    }
}
