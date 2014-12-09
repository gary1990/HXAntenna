using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HxAntenna.Models
{
    public class TestResultItemDegree
    {
        public TestResultItemDegree() {
            this.ResultItemDegree = false;
            this.TestResultItemDegreeVals = new List<TestResultItemDegreeVal> { };
        }
        public int Id { get; set; }
        public int TestResultItemId { get; set; }
        [DisplayName("度数")]
        public decimal Degree { get; set; }
        [DisplayName("测试图像")]
        public string Img { get; set; }
        [DisplayName("测试结果")]
        public bool ResultItemDegree { get; set; }
        public virtual TestResultItem TestResultItem { get; set; }
        public virtual ICollection<TestResultItemDegreeVal> TestResultItemDegreeVals { get; set; }
    }
}