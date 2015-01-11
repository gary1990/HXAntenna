using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HxAntenna.Models.ViewModels
{
    public class TestRecordExport
    {
        public int TestResultId { get; set; }
		public DateTime TestTime { get; set; }
		public string SerialNumber { get; set; }
		public int Result { get; set; }
		public int TestResultItemId { get; set; }
		public string JobNumber { get; set; }
		public int IsEsc { get; set; }
		public int IsLatestTest { get; set; }
		public int ResultItem { get; set; }
		public string TestItemName { get; set; }
		public DateTime TestTimeItem { get; set; }
		public int TestresultItemDegreeId { get; set; }
		public decimal Degree { get; set; }
		public int ResultItemDegree { get; set; }
		public int TestResultItemDegreeValId { get; set; }
		public int Port { get; set; }
        public int ResultItemDegreeVal { get; set; }
        public decimal TestData { get; set; }
        public int Symbol { get; set; }
		public decimal StandardValue { get; set; }
		public int maxport { get; set; }
        public int maxdegree { get; set; }
    }
}