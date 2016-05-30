using System.Collections.Generic;

namespace AFT.RegoV2.Shared.ApiDataFiltering
{
    public class FilteredDataResponse<T>
    {
        public int Page { get; set; }
        public int Total { get; set; }
        public int Records { get; set; }
        public List<T> Rows { get; set; }
    }
}