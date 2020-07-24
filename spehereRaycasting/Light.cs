using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace spehereRaycasting
{
    public class Light
    {
        //directional light
        public Vector3 p { get; set; }
        public Vector4 direction;
        public Color Ip;
        public int r;

        public Light(Vector4 direction, Color ip, int r)
        {
            this.direction = direction;
            Ip = ip;
            this.r = r;
        }
    }
}
