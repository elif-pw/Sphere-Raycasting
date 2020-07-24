using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace spehereRaycasting
{
    public class Sphere
    {
        public float r;
        public Vector4 Centre;
        public Material material;

        public Sphere(float r, float x, float y, float z, Material m)
        {
            this.r = r;
            Centre = new Vector4(x, y, z, 1);
            material = m;
        }
    }

    public class Material
    {
        // r g b
        public Vector3 ka;
        public Vector3 kd;
        public Vector3 ks;
        public float m;

        public Material(Vector3 ka, Vector3 kd, Vector3 ks, float m)
        {
            this.ka = ka;
            this.kd = kd;
            this.ks = ks;
            this.m = m;
        }
    }

}
