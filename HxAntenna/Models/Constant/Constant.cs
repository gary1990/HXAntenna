using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HxAntenna.Models.Constant
{
    public enum Symbol
    {
        LessOrEqual = 0,
        GreatOrEqual = 1,
        Less = 3,
        Great = 4
    }

    public enum MsgType
    {
        OK = 10,
        WARN = 20,
        ERROR = 30,
    }
}