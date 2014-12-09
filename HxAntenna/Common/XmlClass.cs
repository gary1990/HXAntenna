using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace HxAntenna.Common
{
    [XmlRoot("Result")]
    public class SingleResultXml
    {
        public string Message { get; set; }
    }
}