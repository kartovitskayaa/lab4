using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RendererConsole
{
    public static class MatrixHelper
    {
        public static double[,] GetScaleMatrix(double scaleX, double scaleY, double scaleZ) =>
            new double[,] 
            { 
                { scaleX, 0, 0, 0 },
                {0, scaleY, 0, 0 },
                { 0, 0, scaleZ, 0 },
                {0, 0, 0, 1 },
            };

        public static double[,] GetRotationX(double alpha) =>
            new double[,]
            {
                { 1, 0, 0, 0 },
                { 0, Math.Cos(alpha), -Math.Sin(alpha), 0 },
                { 0, Math.Sin(alpha), Math.Cos(alpha), 0 },
                { 0, 0, 0, 1 },
            };

        public static double[,] GetRotationY(double alpha) =>
            new double[,]
            {
                { Math.Cos(alpha), 0, Math.Sin(alpha), 0 },
                { 0, 1, 0, 0 },
                { -Math.Sin(alpha), 0, Math.Cos(alpha), 0 },
                { 0, 0, 0, 1 },
            };

        public static double[,] GetRotationZ(double alpha) =>
            new double[,]
            {
                { Math.Cos(alpha), -Math.Sin(alpha), 0, 0 },
                { Math.Sin(alpha), Math.Cos(alpha), 0, 0 },
                { 0, 0, 1, 0 },
                { 0, 0, 0, 1 },
            };

        public static double[,] GetProjection(double aspect, double fov, double znear, double zfar) =>
            new double[,]
            {
                { 1 / (aspect * Math.Tan(fov / 2)), 0, 0, 0 },
                { 0, 1 / Math.Tan(fov / 2), 0, 0 },
                { 0, 0, zfar / (znear - zfar), -1 },
                { 0, 0, znear * zfar / (znear - zfar), 0 },
            };

        public static double[,] GetViewport(int x, int y, int width, int height) =>
            new double[,]
            {
                { width / 2.0, 0, 0, 0 },
                { 0, -height / 2, 0, 0 },
                { 0, 0, 1, 0 },
                { x + width / 2.0, y + height / 2.0, 0, 1 },
            };

        public static double[,] Multiply(this double[,] a, double[,] b)
        {
            int size = a.GetUpperBound(0) + 1;
            var result = new double[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    double sum = 0;
                    for (int r = 0; r < size; r++)
                    {
                        sum += a[i, r] * b[r, j];
                    }

                    result[i, j] = sum;
                }
            }

            return result;
        }

        public static Vertex Multiply(this double[,] matrix, Vertex vector)
        {
            return new Vertex(
                matrix[0, 0] * vector.X + matrix[0, 1] * vector.Y + matrix[0, 2] * vector.Z + matrix[0, 3],
                matrix[1, 0] * vector.X + matrix[1, 1] * vector.Y + matrix[1, 2] * vector.Z + matrix[1, 3],
                matrix[2, 0] * vector.X + matrix[2, 1] * vector.Y + matrix[2, 2] * vector.Z + matrix[2, 3]);
        }
    }
}
