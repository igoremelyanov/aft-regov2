using System;

namespace AFT.RegoV2.Shared.ApiDataFiltering
{
    public class FilteredDataRequest
    {
        /// <summary>
        /// the requested page
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// the number of rows requested
        /// </summary>
        public int RowCount { get; set; }

        /// <summary>
        /// Top Records
        /// </summary>
        public int TopRecords { get; set; }

        /// <summary>
        /// the sorting column
        /// </summary>
        public string SortColumn { get; set; }

        /// <summary>
        /// the sort order
        /// </summary>
        public string SortSord { get; set; }

        public Filter[] Filters { get; set; }
    }

    public class Filter
    {
        public string Field { get; set; }
        public ComparisonOperator Comparison { get; set; }
        public string Data { get; set; }
    }

    [Flags]
    public enum ComparisonOperator
    {
        /// <summary>
        /// the value of the search indicator is false
        /// </summary>
        None = 0,
        /// <summary>
        /// equal
        /// </summary>
        Eq = 0x0001,
        /// <summary>
        /// not equal
        /// </summary>
        Ne = 0x0002,
        /// <summary>
        /// less than
        /// </summary>
        Lt = 0x0004,
        /// <summary>
        /// less or equal
        /// </summary>
        Le = 0x0008,
        /// <summary>
        /// greater than
        /// </summary>
        Gt = 0x0010,
        /// <summary>
        /// greater or equal
        /// </summary>
        Ge = 0x0020,
        /// <summary>
        /// begins with
        /// </summary>
        Bw = 0x0040,
        /// <summary>
        /// does not begin with
        /// </summary>
        Bn = 0x0080,
        /// <summary>
        /// is in
        /// </summary>
        In = 0x0100,
        /// <summary>
        /// is not in
        /// </summary>
        Ni = 0x0200,
        /// <summary>
        /// ends with
        /// </summary>
        Ew = 0x0400,
        /// <summary>
        /// does not end with
        /// </summary>
        En = 0x0800,
        /// <summary>
        /// contains
        /// </summary>
        Cn = 0x1000,
        /// <summary>
        /// does not contain
        /// </summary>
        Nc = 0x2000
    }
}
