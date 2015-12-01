using HxAntenna.Interface;
using HxAntenna.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HxAntenna.Models
{
    public class TestEquipment : BaseModel, IEditable<TestEquipment>
    {
        [DisplayName("序列号")]
        public string SerialNumber { get; set; }
        public bool isVna { get; set; }

        public void Edit(TestEquipment model)
        {
            this.Name = model.Name;
            this.SerialNumber = model.SerialNumber;
        }
    }
}