using HxAntenna.Interface;
using HxAntenna.Models.Base;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HxAntenna.Models
{
    public class AntennaRole : BaseModel, IEditable<AntennaRole>
    {
        public AntennaRole() {
            this.Permissions = new List<Permission> { };
        }
        [DisplayName("权限")]
        public virtual ICollection<Permission> Permissions { get; set; }

        public void Edit(AntennaRole model)
        {
            this.Name = model.Name;
            this.Permissions = model.Permissions;
        }
    }
}