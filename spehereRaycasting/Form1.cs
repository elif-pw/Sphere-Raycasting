using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathNet.Numerics.LinearAlgebra;

namespace spehereRaycasting
{
    public partial class Form1 : Form
    {
        Bitmap bmp;
        List<Sphere> spheres;
        Vector4[] M;
        Light light;
        int angX = 0;
        int angY = 0;
        Vector4 camPosition;
        Vector4 targetPosition;
        Vector3 up;
        public Form1()
        {
           // this.KeyPreview = true;
            InitializeComponent();
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            // Camera Matrix
            camPosition = new Vector4(0,0, 10, 1);
            targetPosition = new Vector4(0,0,-100, 1);
            up = new Vector3(0, 1, 0);
            M = camera();

            Vector4 light_direction = new Vector4(0.5f, -10, -1, 0);
            light = new Light(Vector4.Divide(light_direction, light_direction.Length()), Color.White, 10);


            //initiation of the spheres
            Sphere sphere = new Sphere(0.01f, 0, 0, 0, new Material(
              new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f),
              new Vector3(0f, 0f, 0f), 0
              ));

            Sphere sphere5 = new Sphere(2, 0, 0, 0, new Material(
                new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f), 
                new Vector3(1f, 1f, 1f), 50
                ));

            Sphere sphere1 = new Sphere(1, -3, -3, 0, new Material(
                new Vector3(0.9f, 0.5f, 0.5f), new Vector3(0.9f, 0.5f, 0.5f),
                new Vector3(1f, 1f,1f), 50
                ));
            Sphere sphere2 = new Sphere(1, 3, -3, 0, new Material(
              new Vector3(0.1f, 0.5f, 0.5f), new Vector3(0.9f, 0.8f, 0.5f),
              new Vector3(1f, 1f, 1f), 50
              ));
            Sphere sphere3 = new Sphere(1, 3, 3, 0, new Material(
             new Vector3(0.1f, 0.5f, 0.9f), new Vector3(1f, 0.8f, 0.2f),
             new Vector3(1f, 1f, 1f), 50
             ));
            Sphere sphere4 = new Sphere(1, -3, 3, 0, new Material(
            new Vector3(0.0f, 0.0f, 0.9f), new Vector3(0.9f, 0.9f, 0.9f),
            new Vector3(0.9f, 0.9f, 0.9f), 50
            ));

            spheres = new List<Sphere>();
            spheres.Add(sphere);
            spheres.Add(sphere1);
            spheres.Add(sphere2);
            spheres.Add(sphere3);
            spheres.Add(sphere4);
            spheres.Add(sphere5);
            ray_casting();
           
        }


        public unsafe void ray_casting()
        {
          
            //lock bits
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData =
                bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            byte* firstpix = (byte*)bmpData.Scan0;

            float Cx = (bmp.Width - 1) / 2;
            float Cy = (bmp.Height - 1) / 2;

            Vector4 pstart = new Vector4(0, 0, 0, 1);
            Vector4 p = MxV(M, pstart);
            // angle
            int theta = 90;
            float radians = theta * (float)(Math.PI / 180);
            float d = (bmp.Width / 2 * (1f / (float)Math.Tan(radians / 2)));

            for (int y = 0; y < bmp.Height; y++)
            {
                byte* temp = firstpix + (y * bmpData.Stride);
                for (int x = 0; x < bmp.Width; x++)
                {
                                   
                    Vector4 q = new Vector4(x - Cx, -y + Cy, -d, 1);
                    Vector4 vdirection = (q - pstart) / (q - pstart).Length();
                    Vector4 v = MxV(M, vdirection);
                    Vector4[] npt = ray_intersect(p, v);
                    if (npt != null && npt[2].X>-1)
                    {
                        Color c = point_color(spheres[(int)npt[2].X], light, camPosition, npt[0], npt[1]);
                        temp[x * 3] = c.R;
                        temp[x * 3 + 1] = c.G;
                        temp[x * 3 + 2] = c.B;
                    }
               

                }
            }
            //unlock bits
            bmp.UnlockBits(bmpData);
            pictureBox1.Image = bmp;

        }

        public Vector4 translateY(Vector4 p, int t)
        {
           
            Vector4 w = new Vector4(
                p.X,
                p.Y + p.W*t,
                p.Z,
                p.W
                );

            return w;
        }
        public Vector4 translateX(Vector4 p, int t)
        {
            Vector4 w = new Vector4(
                p.X+ p.W*t,
                p.Y ,
                p.Z,
                p.W
                );

            return w;
        }


        //rotation around Y axes
        public Vector4 rotateY(Vector4 p)
        {
            int theta = angY;
            float radians = theta * (float)(Math.PI / 180);
            Vector4 w = new Vector4(
                p.X * (float)Math.Cos(radians) + p.Z * (float)Math.Sin(radians),
                p.Y,
                (-1) * (float)Math.Sin(radians) * p.X + p.Z * (float)Math.Cos(radians),
                p.W
                );
            return w;
        }
        //rotation around X axes
        public Vector4 rotateX(Vector4 p)
        {

            int theta = angX;
            float radians = theta * (float)(Math.PI / 180);
            Vector4 w = new Vector4(
                p.X,
                p.Y * (float)Math.Cos(radians) + p.Z*(-1) * (float)Math.Sin(radians),
                p.Y* (float)Math.Sin(radians)  + p.Z * (float)Math.Cos(radians),
                p.W
                );

            return w;
        }

        public Vector4[] camera()
        {
      
            var target = new Vector3(targetPosition.X, targetPosition.Y, targetPosition.Z);
            var camera = new Vector3(camPosition.X, camPosition.Y, camPosition.Z);
            var cZ = (camera - target) / (camera - target).Length();
            var cX = Vector3.Cross(up, cZ) / Vector3.Cross(up, cZ).Length();
            var cY = Vector3.Cross(cZ, cX) / Vector3.Cross(cZ, cX).Length();
            Vector4[] Matrix = {
                new Vector4(cX.X, cX.Y, cX.Z, Vector3.Dot(cX, camera)),
                new Vector4(cY.X, cY.Y, cY.Z, Vector3.Dot(cY, camera)),
               new Vector4(cZ.X, cZ.Y, cZ.Z, Vector3.Dot(cZ, camera)),
                new Vector4(0, 0, 0, 1) };
            return Matrix;
        }
        public Color point_color(Sphere sphere, Light light, Vector4 pc, Vector4 n, Vector4 pt)
        {
            Vector4 v = Vector4.Divide(Vector4.Subtract(pc, pt), Vector4.Subtract(pc, pt).Length());
            Vector4 l = Vector4.Multiply(light.direction, -1);
            Vector4 r = Vector4.Subtract(Vector4.Multiply(n, 2 * Vector4.Dot(n, l)), l);
            Color I = light.Ip;

            float Ir = I.R * sphere.material.ka.X + sphere.material.kd.X * I.R * Math.Max(0, Vector4.Dot(n, l)) + sphere.material.ks.X * I.R * (float)Math.Pow(Math.Max(0, Vector4.Dot(v, r)), sphere.material.m);
            float Ig = I.G * sphere.material.ka.Y + sphere.material.kd.Y * I.G * Math.Max(0, Vector4.Dot(n, l)) + sphere.material.ks.Y * I.G * (float)Math.Pow(Math.Max(0, Vector4.Dot(v, r)), sphere.material.m);
            float Ib = I.B * sphere.material.ka.Z + sphere.material.kd.Z * I.B * Math.Max(0, Vector4.Dot(n, l)) + sphere.material.ks.Z * I.B * (float)Math.Pow(Math.Max(0, Vector4.Dot(v, r)), sphere.material.m);

            if (Ir > 255) Ir = 255;
            else if (Ir < 0) Ir = 0;
            if (Ig > 255) Ig = 255;
            else if (Ig < 0) Ig = 0; 
            if (Ib > 255) Ib = 255;
            else if (Ib < 0) Ig = 0; 

            Color c = Color.FromArgb((int)Ir, (int)Ig, (int)Ib);
            return c;
        }

        public Vector4[] ray_intersect(Vector4 p, Vector4 v)
        {
            Vector4[] npt =new Vector4[3];
            double? tmin=double.MaxValue;
            for (int j = 0; j < spheres.Count; j++)
            {
                
                float b = 2 * Vector4.Dot(v, Vector4.Subtract(p, spheres[j].Centre));
                float c = Vector4.Subtract(p, spheres[j].Centre).Length() * Vector4.Subtract(p, spheres[j].Centre).Length() - spheres[j].r * spheres[j].r;
                double? t1 = find_root(1, b, c, 1);
                double? t2 = find_root(1, b, c, -1);
                double? t = root_check(t1, t2);
                if (t1 != null && t2 !=null)
                {
                    if (t < tmin && t != null && tmin != null && t.HasValue)
                    {
                        tmin = t;
                        //pt = p + tv
                        Vector4 point = new Vector4(
                                p.X + (float)t * v.X,
                                p.Y + (float)t * v.Y,
                                p.Z + (float)t * v.Z,
                                p.W + (float)t * v.W
                                );
                        //normal vector to the sphere at that point
                        Vector4 n = Vector4.Divide(Vector4.Subtract(point, spheres[j].Centre), Vector4.Subtract(point, spheres[j].Centre).Length());
                        npt = new Vector4[] { n, point, new Vector4(j, -1, -1, -1) };
                    }
                }
                
               
               
               
            }
            return npt;

        }
        static double? find_root(float a, float b, float c, int sgn)
        {
            var root = b * b - 4 * a * c;
            if (root <= 0)
                return null;
            else
                return (sgn * Math.Sqrt(root) - b) / (2.0 * a);
        }

        public double? root_check(double? t1, double? t2)
        {          
            if (t1==null || t1 < 0 || t1==t2) return null;
            else
            {
                if (t2==null || t2 < 0) return null;
                else
                {
                    if (t1 < t2) return t1;
                    else return t2;
                }
            }            
        }
        public Vector4 MxV(Vector4[] M, Vector4 V)
        {
            return new Vector4(
             M[0].X * V.X+ M[0].Y * V.Y + M[0].Z * V.Z + M[0].W * V.W,
             M[1].X * V.X + M[1].Y * V.Y + M[1].Z * V.Z + M[1].W * V.W,
             M[2].X * V.X + M[2].Y * V.Y + M[2].Z * V.Z + M[2].W * V.W,
             M[3].X * V.X + M[3].Y * V.Y + M[3].Z * V.Z + M[3].W * V.W
            );
            
        }

        //camera rotate up, down, left or right
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
          
            if (keyData == Keys.Up)
            {
                black_out();
                angX = 30;
                targetPosition = rotateX(targetPosition);
                M = camera();

                ray_casting();
                return true;
            }
        
            if (keyData == Keys.Down)
            {
                black_out();

                angX = -30;
                targetPosition = rotateX(targetPosition);

                M = camera();

                ray_casting();
                return true;
            }
            if (keyData == Keys.Left)
            {
                black_out();
                angY = 30;
                targetPosition = rotateY(targetPosition);

                M = camera();
                ray_casting();
                return true;
            }
            if (keyData == Keys.Right)
            {
                black_out();
                angY = -30;
                targetPosition = rotateY(targetPosition);

                M = camera();
                ray_casting();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        
        public unsafe void black_out()
        {
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData =
                bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            byte* firstpix = (byte*)bmpData.Scan0;


            for (int y = 0; y < bmp.Height; y++)
            {
                byte* temp = firstpix + (y * bmpData.Stride);
                for (int x = 0; x < bmp.Width; x++)
                {
                    temp[x * 3] = 0;
                    temp[x * 3 + 1] = 0;
                    temp[x * 3 + 2] = 0;
   
                }
            }
            bmp.UnlockBits(bmpData);
            pictureBox1.Image = bmp;
        }

        //zoom in or out 
        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'i')
            {
                black_out();
                camPosition.Z = camPosition.Z -5;
                M = camera();
                ray_casting();
                
            }
            if (e.KeyChar == 'o')
            {
                black_out();
                camPosition.Z = camPosition.Z +5;
                M = camera();
                ray_casting();
               
            }

        }
    }
}
