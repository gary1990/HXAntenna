using HxAntenna.Interface;
using HxAntenna.Models.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HxAntenna.Models
{
    public class TestItem : BaseModel, IEditable<TestItem>
    {
        public void Edit(TestItem model)
        {
            this.Name = model.Name;
        }
    }
}