using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveformCanvasSample
{
    // RGB를 Normalized 한 값으로 갖고 있는 객체
    public class RGBcolor
    {
        const float MaxByte = byte.MaxValue;

        float r;
        float g;
        float b;

        public float R
        {
            get { return r; }
            private set { r = value; }
        }

        public float G
        {
            get { return g; }
            private set { g = value; }
        }

        public float B
        {
            get { return b; }
            private set { b = value; }
        }

        public RGBcolor(float r, float g, float b)
        {
            this.R = r / MaxByte;
            this.G = g / MaxByte;
            this.B = b / MaxByte;
        }

        public RGBcolor(Color c)
        {
            this.R = c.R / MaxByte;
            this.G = c.G / MaxByte;
            this.B = c.B / MaxByte;
        }
    }
}
