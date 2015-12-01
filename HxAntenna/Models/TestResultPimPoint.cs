using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HxAntenna.Models
{
    public class TestResultPimPoint
    {
        public TestResultPimPoint()
        {
            isWorst = false;
        }
        public int Id { get; set; }
        public int TestResultPimId { get; set; }
        public virtual TestResultPim TestResultPim { get; set; }
        [DisplayName("Carrier 1")]
        public decimal CarrierOneFreq { get; set; }
        [DisplayName("Carrier 2")]
        public decimal CarrierTwoFreq { get; set; }
        [DisplayName("IM Freq")]
        public decimal ImFreq { get; set; }
        [DisplayName("IM Power")]
        public decimal ImPower { get; set; }
        public bool isWorst { get; set; }
    }
}