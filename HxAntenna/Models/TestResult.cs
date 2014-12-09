using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HxAntenna.Models
{
    public class TestResult
    {
        public TestResult()
        {
            this.Result = false;
            this.TestResultItems = new List<TestResultItem> { };
        }
        public int Id { get; set; }
        [DisplayName("序列号")]
        public int SerialNumberId { get; set; }
        [DisplayName("测试结果")]
        public bool Result { get; set; }
        [DisplayName("测试时间")]
        public DateTime TestTime { get; set; }
        public virtual SerialNumber SerialNumber { get; set; }
        public virtual ICollection<TestResultItem> TestResultItems { get; set; }
    }
}