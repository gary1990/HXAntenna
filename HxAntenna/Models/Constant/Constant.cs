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
    public enum Unit
    {
        G = 5,//10^9
        M = 6,//10^6
        k = 7,//10^3
        h = 8,//10^2 百
        da = 9,//10^1 十
        d = 10,//10^-1 分
        c = 11,//10^-2 厘
        m = 12,//10^-3 毫
        μ = 13,//10^-6 微
        n = 14//10^-9 纳
    }

    public enum ImUnit
    {
        dBm = 1,
        dBc = 5
    }
    public enum TestMeans
    {
        Single = 1,
        Sweep = 5
    }
}