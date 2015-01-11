using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HxAntenna.Models.ViewModels
{
    public class TestitemStandardPortGoup
    {
        public string TestItemName { get; set; }
        public int Symbol { get; set; }
        public decimal StandardValue { get; set; }
        public int Port { get; set; }
        public int RowNumber { get; set; }
        public int CellNumber { get; set; }
        public TestitemStandardPortGoup() 
        {
            //because row in excel is always 3
            this.RowNumber = 2;
        }
    }
}