using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace RendererConsole
{
    public struct Vertex
    {
        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }

        public double W { get; set; }

        public Vertex(double x, double y, double z, double w = 1)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        public Vertex Normalize()
        {
            double length = Math.Sqrt(X * X + Y * Y + Z * Z);

            return new Vertex(X / length, Y / length, Z / length);
        }

        public Vertex Substract(Vertex v)
        {
            return new Vertex(X - v.X, Y - v.Y, Z - v.Z);
        }

        public Vertex Multily(double a)
        {
            return new Vertex(a * X, a * Y, a * Z);
        }

        public Vertex CrossProduct(Vertex v)
        {
            return new Vertex(Y * v.Z - Z * v.Y, Z * v.X - X * v.Z, X * v.Y - Y * v.X);
        }

        public double DotProduct(Vertex v)
        {
            return X * v.X + Y * v.Y + Z * v.Z;
        }

        public Vertex Add(Vertex v)
        {
            return new Vertex(X + v.X, Y + v.Y, Z + v.Z);
        }

        public Vertex Div(double divider)
        {
            return new Vertex(X / divider, Y / divider, Z / divider);
        }
    }
}
