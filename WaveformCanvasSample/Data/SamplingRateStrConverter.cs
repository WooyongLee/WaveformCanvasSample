using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveformCanvasSample
{
    public enum ESamplingRate
    {
        None = -1,
        _EA6000,
        _1D4C000,
        _3A98000,
        _7530000,
        _EA60000
    }

    public static class SamplingRateStrConverter
    {
        public static ESamplingRate GetSamplingRate(string strData)
        {
            ESamplingRate eSamplingRate = ESamplingRate.None;
            strData = strData.TrimStart('0');
            if (strData == "EA6000" || strData == "15360000")
            {
                return ESamplingRate._EA6000;
            }

            if (strData == "1D4C000" || strData == "30720000")
            {
                return ESamplingRate._1D4C000;
            }

            if (strData == "3A98000" || strData == "61440000")
            {
                return ESamplingRate._3A98000;
            }

            if (strData == "7530000" || strData == "122880000")
            {
                return ESamplingRate._7530000;
            }

            if (strData == "EA60000" || strData == "245760000")
            {
                return ESamplingRate._EA60000;
            }

            return ESamplingRate.None;
        }
    }
}
