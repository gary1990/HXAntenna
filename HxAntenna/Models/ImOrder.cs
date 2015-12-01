using HxAntenna.Models.Constant;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HxAntenna.Models
{
    public class ImOrder
    {
        public int Id { get; set; }
        [DisplayName("阶数")]
        public int OrderNumber { get; set; }
        public ImUnit ImUnit { get; set; }
    }
}