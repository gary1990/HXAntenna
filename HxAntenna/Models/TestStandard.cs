using HxAntenna.Interface;
using HxAntenna.Models.Base;
using HxAntenna.Models.Constant;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HxAntenna.Models
{
    public class TestStandard : IEditable<TestStandard>
    {
        public int Id { get; set;}
        [DisplayName("比较符")]
        public Symbol Symbol { get; set; }
        [DisplayName("标准值")]
        public decimal StandardValue { get; set; }
        public void Edit(TestStandard model)
        {
            this.StandardValue = model.StandardValue;
            this.Symbol = model.Symbol;
        }
    }
}