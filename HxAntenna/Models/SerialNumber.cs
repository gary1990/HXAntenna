using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HxAntenna.Models
{
    public class SerialNumber
    {
        public SerialNumber(){
            
        }
        public int Id { get; set; }
        [Required]
        [DisplayName("名称")]
        [MaxLength(100)]
        public string Name { get; set; }
    }
}