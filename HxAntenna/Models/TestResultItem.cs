using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HxAntenna.Models
{
    public class TestResultItem
    {
        public TestResultItem() {
            this.ResultItem = false;
            this.IsLatestTest = false;
            this.IsEsc = false;
            this.TestResultItemDegrees = new List<TestResultItemDegree> { };
        }
        public int Id { get; set; }
        public int TestResultId { get; set; }
        [DisplayName("测试项结果")]//false is FAIL, true is PASS
        public bool ResultItem { get; set; }
        [DisplayName("测试项")]
        public int TestItemId { get; set; }
        [DisplayName("测试员")]
        public string AntennaUserId { get; set; }
        [DisplayName("测试时间")]
        public DateTime TestTimeItem { get; set; }
        [DisplayName("最新记录")]//false is not lastest, 0. true is latest, 1.
        public bool IsLatestTest { get; set; }
        [DisplayName("电调")]//false is not ESC, true is NOT ESC
        public bool IsEsc { get; set; }
        public virtual AntennaUser AntennaUser { get; set; }
        public virtual TestItem TestItem { get; set; }
        public virtual TestResult TestResultInfo { get; set; }
        public virtual ICollection<TestResultItemDegree> TestResultItemDegrees { get; set; }
    }
}