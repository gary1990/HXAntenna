using HxAntenna.Models.Constant;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HxAntenna.Models
{
    public class Carrier
    {
        public Carrier()
        {
            TestResultPims = new List<TestResultPim> { };
        }
        public int Id { get; set; }
        [DisplayName("固定频率")]
        public decimal SetFreq { get; set; }
        [DisplayName("起始频率")]
        public decimal StartFreq { get; set; }
        [DisplayName("终止频率")]
        public decimal StopFreq { get; set; }
        public Unit FreqUnit { get; set; }
        [DisplayName("输出功率")]
        public decimal Power { get; set; }
        public ImUnit ImUnit { get; set; }
        public virtual ICollection<TestResultPim> TestResultPims { get; set; }
    }
}