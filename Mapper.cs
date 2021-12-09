using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RendererConsole
{
    class Mapper
    {
        public static Vector4 VertexToVector4(Vertex v) =>
            new Vector4((float)v.X, (float)v.Y, (float)v.Z, 1);

        public static Matrix4x4 MatrixToMatrix4x4(double[,] mvp) =>
            new Matrix4x4(
                        (float)mvp[0, 0], (float)mvp[0, 1], (float)mvp[0, 2], (float)mvp[0, 3],
                        (float)mvp[1, 0], (float)mvp[1, 1], (float)mvp[1, 2], (float)mvp[1, 3],
                        (float)mvp[2, 0], (float)mvp[2, 1], (float)mvp[2, 2], (float)mvp[2, 3],
                        (float)mvp[3, 0], (float)mvp[3, 1], (float)mvp[3, 2], (float)mvp[3, 3]);

        public static Vertex Vector4ToVertex(Vector4 v) =>
            new Vertex() { X = v.X, Y = v.Y, Z = v.Z, W = v.W };

        public static Vector3 VertexToVector3(Vertex v) =>
            new Vector3((float)v.X, (float)v.Y, (float)v.Z);

        public static Vertex Vector3ToVertex(Vector3 v) =>
            new Vertex(v.X, v.Y, v.Z);
    }
}
