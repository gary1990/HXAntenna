using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HxAntenna.Models
{
    public class TestResultItemDegreeVal
    {
        public TestResultItemDegreeVal() {
            this.ResultItemDegreeVal = false;
        }
        public int Id { get; set; }
        public int TestResultItemDegreeId { get; set; }
        public int TestStandardId { get; set; }
        [DisplayName("端口")]
        public int Port { get; set; }
        [DisplayName("测试值")]
        public decimal TestData { get; set; }
        public bool ResultItemDegreeVal { get; set; }
        public virtual TestResultItemDegree TestResultItemDegree { get; set; }
        public virtual TestStandard TestStandard { get; set; }
    }
}