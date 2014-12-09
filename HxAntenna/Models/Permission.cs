using HxAntenna.Interface;
using HxAntenna.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HxAntenna.Models
{
    public class Permission : BaseModel, IEditable<Permission>
    {
        public Permission() 
        {
            this.AntennaRoles = new List<AntennaRole> { };
        }
        [Required]
        [DisplayName("控制器名")]
        public string ControllerName { get; set; }
        [DisplayName("方法名")]
        public string ActionName { get; set; }
        [DisplayName("权限")]
        public virtual ICollection<AntennaRole> AntennaRoles { get; set; }
        public void Edit(Permission model)
        {
            this.Name = model.Name;
            this.ControllerName = model.ControllerName;
            this.ActionName = model.ActionName;
        }
    }
}