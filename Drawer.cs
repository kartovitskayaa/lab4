using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;

namespace RendererConsole
{
    public class Drawer
    {

        private static Random rand = new Random();
        private static double[] ZBuffer;
        private static int[] bloomBuffer;

        private static int shiness = 32;

        public static unsafe Bitmap FillTexturedTriangle(
            Bitmap canvas,
            Camera camera,
            Vertex lightPos,
            Vertex[][] triangles,
            Vertex[][] worldTriangles,
            Vertex[][] normals,
            Vertex[][] textures,
            int[,] diffuseMap,
            int[,] specularMap,
            int[,] emissionMap,
            Vector3[,] normalMap)
        {
            BitmapData sourceData = canvas.LockBits(new Rectangle(0, 0,
                canvas.Width, canvas.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int* ptrSourceData = (int*)sourceData.Scan0.ToPointer();

            int mapWidth = emissionMap.GetUpperBound(1) + 1;

            int tmStart = Environment.TickCount;
            if (ZBuffer == null || ZBuffer.Length != canvas.Height * canvas.Width)
            {
                ZBuffer = new double[canvas.Height * canvas.Width];
            }

            if (bloomBuffer == null || bloomBuffer.Length != canvas.Height * canvas.Width)
            {
                bloomBuffer = new int[canvas.Height * canvas.Width];
            }

            ZBuffer = ZBuffer.AsParallel().Select(_ => double.MaxValue).ToArray();
            bloomBuffer = bloomBuffer.AsParallel().Select(_ => 0).ToArray();

            int tmEnd = Environment.TickCount;
            Console.WriteLine($"Unlock: {tmEnd - tmStart}");

            int elapsedTriangle = 0;
            int elapsedDot = 0;
            for (int i = 0; i < triangles.Length; i++)
            {
                int tm1, tm2;
                Vector4[] triangle = triangles[i].Select(v => new Vector4((float)v.X, (float)v.Y, (float)v.Z, (float)v.W)).ToArray();
                Vector4[] worldTriangle = worldTriangles[i].Select(v => new Vector4((float)v.X, (float)v.Y, (float)v.Z, 1)).ToArray();
                Vector4[] normalsTriangle = normals[i].Select(v => new Vector4((float)v.X, (float)v.Y, (float)v.Z, 1)).ToArray();
                Vector4[] texturesTriangle = textures[i].Select(v => new Vector4((float)v.X, (float)v.Y, (float)v.Z, 0)).ToArray();

                //if (normals[i][0].DotProduct(camera.Direction) < 0)
                //{
                //    continue;
                //}

                tm1 = Environment.TickCount;
                DrawTexturedTriangle(
                    triangle, 
                    worldTriangle, 
                    normalsTriangle, 
                    texturesTriangle, 
                    diffuseMap, 
                    specularMap, 
                    emissionMap,
                    normalMap, 
                    ptrSourceData, 
                    canvas.Width, 
                    canvas.Height, 
                    ZBuffer, 
                    camera, 
                    Mapper.VertexToVector3(lightPos));
                //      DrawGradientTriangle(triangle, worldTriangle, normalsTriangle, ptrSourceData, canvas.Width, canvas.Height, ZBuffer, camera, Mapper.VertexToVector3(lightPos));
                
                tm2 = Environment.TickCount;

                elapsedTriangle += tm2 - tm1;
            }
            
            DrawBloom(bloomBuffer, canvas.Width, canvas.Height);

            for (int j = 0; j < bloomBuffer.Length; j++)
            {
                ptrSourceData[j] += bloomBuffer[j];
            }

            Console.WriteLine($"Elpsed dot: {elapsedDot}");
            Console.WriteLine($"Elapsed traingle: {elapsedTriangle}");
            canvas.UnlockBits(sourceData);

            return canvas;
        }

        private static double GetBrightness(int value)
        {
            Color color = Color.FromArgb(value);

            return 0.2126 * color.A + 0.7152 * color.G + 0.0722 * color.B;
        }

        private static unsafe void DrawBloom(int[] buffer, int width, int height)
        {
            int m = 15;
            double[] kernel = Filters.GetGaussKernel(m);

            HelpDraw(true);
            HelpDraw(false);
                
            void HelpDraw(bool horizontal)
            {
                for (int i = m; i < buffer.Length - m; i++)
                {
                    double red = 0;
                    double green = 0;
                    double blue = 0;
                    for (int j = -m; j < m; j++)
                    {
                        int index = i + (horizontal ? j : j * width);
                        index = Math.Min(Math.Max(index, 0), width * height - 1);

                        Color temp = Color.FromArgb(buffer[index]);
                        red += kernel[j + m] * temp.R;
                        green += kernel[j + m] * temp.G;
                        blue += kernel[j + m] * temp.B;
                    }

                    buffer[i] = Color.FromArgb(Math.Min((int)red, 255), Math.Min((int)green, 255), Math.Min((int)blue, 255)).ToArgb(); ;
                }
            }
        }

        public static unsafe Bitmap FillTrianglesPhong(
            Bitmap canvas, 
            Camera camera, 
            Vertex lightPos, 
            Vertex[][] triangles, 
            Vertex[][] worldTriangles,
            Vertex[][] normals, 
            Color color)
        {
            BitmapData sourceData = canvas.LockBits(new Rectangle(0, 0,
             canvas.Width, canvas.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int* ptrSourceData = (int*)sourceData.Scan0.ToPointer();

            int tmStart = Environment.TickCount;
            if (ZBuffer == null || ZBuffer.Length != canvas.Height * canvas.Width)
            {
                ZBuffer = new double[canvas.Height * canvas.Width];
            }

            ZBuffer = ZBuffer.AsParallel().Select(_ => double.MaxValue).ToArray();

            int tmEnd = Environment.TickCount;
            Console.WriteLine($"Unlock: {tmEnd - tmStart}");

            int elapsedTriangle = 0;
            int elapsedDot = 0;
            for (int i = 0; i < triangles.Length; i++)
            {
                int tm1, tm2;
                Vector4[] triangle = triangles[i].Select(v => new Vector4((float)v.X, (float)v.Y, (float)v.Z, 1)).ToArray();
                Vector4[] worldTriangle = worldTriangles[i].Select(v => new Vector4((float)v.X, (float)v.Y, (float)v.Z, 1)).ToArray();
                Vector4[] normalsTriangle = normals[i].Select(v => new Vector4((float)v.X, (float)v.Y, (float)v.Z, 1)).ToArray();

                //if (normals[i][0].DotProduct(camera.Direction) < 0)
                //{
                //    continue;
                //}

                tm1 = Environment.TickCount;
                DrawGradientTriangle(triangle, worldTriangle, normalsTriangle, ptrSourceData, canvas.Width, canvas.Height, ZBuffer, camera, Mapper.VertexToVector3(lightPos));

                tm2 = Environment.TickCount;

                elapsedTriangle += tm2 - tm1;
            }

            Console.WriteLine($"Elpsed dot: {elapsedDot}");
            Console.WriteLine($"Elapsed traingle: {elapsedTriangle}");
            canvas.UnlockBits(sourceData);

            return canvas;
        }

        private static int Max(int a, int b) => a > b ? a : b;

        private static double Max(double a, double b) => a > b ? a : b;

        private static int Min(int a, int b) => a < b ? a : b;

        private static double Min(double a, double b) => a < b ? a : b;

        public static unsafe Bitmap FillTrianglePhong(Bitmap canvas, Vertex[] points, Vertex[] normals, Vertex lightPos, Color color)
        {
            BitmapData sourceData = canvas.LockBits(new Rectangle(0, 0,
             canvas.Width, canvas.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int* ptrSourceData = (int*)sourceData.Scan0.ToPointer();

            if (ZBuffer == null || ZBuffer.Length != canvas.Height * canvas.Width)
            {
                ZBuffer = new double[canvas.Height * canvas.Width];
            }

            ZBuffer = ZBuffer.AsParallel().Select(_ => double.MaxValue).ToArray();

       //     FillTrianglePhong(ptrSourceData, ZBuffer, canvas.Width, canvas.Height, points, normals, lightPos, color, 0.5);

            canvas.UnlockBits(sourceData);
            return canvas;
        }

        private static void SwapPoints(ref Vector4 a, ref Vector4 b) => (a, b) = (b, a);

        private static unsafe void DrawGradientTriangle(
            Vector4[] modelVertices, 
            Vector4[] worldVertices, 
            Vector4[] worldNormalVectors, 
            int* source, 
            int width, 
            int height,
            double[] zBuffer,
            Camera camera,
            Vector3 lightPosition)
        {
            Vector4 v0 = modelVertices[0];// new Vector4(modelVertices[polygon.VertexIndices[0] - 1]);
            Vector4 v1 = modelVertices[1];// new Vector4(modelVertices[polygon.VertexIndices[1] - 1]);
            Vector4 v2 = modelVertices[2];// new Vector4(modelVertices[polygon.VertexIndices[2] - 1]);

            Vector4 p0 = worldVertices[0];//new Vector4(worldVertices[polygon.VertexIndices[0] - 1]);
            Vector4 p1 = worldVertices[1];
            Vector4 p2 = worldVertices[2];

            Vector4 n0 = worldNormalVectors[0];// new Vector4(worldNormalVectors[polygon.NormalIndices[0] - 1]);
            Vector4 n1 = worldNormalVectors[1];// new Vector4(worldNormalVectors[polygon.NormalIndices[1] - 1]);
            Vector4 n2 = worldNormalVectors[2];// new Vector4(worldNormalVectors[polygon.NormalIndices[2] - 1]);

            if ((int)Math.Ceiling(v0.Y) > (int)Math.Ceiling(v1.Y))
            {
                SwapPoints(ref v0, ref v1);
                SwapPoints(ref n0, ref n1);
                SwapPoints(ref p0, ref p1);
            }

            if ((int)Math.Ceiling(v1.Y) > (int)Math.Ceiling(v2.Y))
            {
                SwapPoints(ref v1, ref v2);
                SwapPoints(ref n1, ref n2);
                SwapPoints(ref p1, ref p2);
            }


            if ((int)Math.Ceiling(v0.Y) > (int)Math.Ceiling(v1.Y))
            {
                SwapPoints(ref v0, ref v1);
                SwapPoints(ref n0, ref n1);
                SwapPoints(ref p0, ref p1);
            }

            int p0y = (int)Math.Ceiling(v0.Y);
            int p1y = (int)Math.Ceiling(v1.Y);
            int p2y = (int)Math.Ceiling(v2.Y);

            int total_height = p2y - p0y;

            for (int i = 0; i < total_height; i++)
            {
                bool second_half = i > p1y - p0y || p1y == p0y;
                int segment_height = second_half ? p2y - p1y : p1y - p0y;
                if (segment_height == 0)
                {
                    continue;
                }

                float alpha = (float)i / total_height;
                float beta = (float)(i - (second_half ? p1y - p0y : 0)) / segment_height;

                Vector4 A = v0 + (v2 - v0) * alpha;
                Vector4 B = second_half ? v1 + (v2 - v1) * beta : v0 + (v1 - v0) * beta;

                Vector4 An = n0 + (n2 - n0) * alpha;
                Vector4 Bn = second_half ? n1 + (n2 - n1) * beta : n0 + (n1 - n0) * beta;

                Vector4 Aw = p0 + (p2 - p0) * alpha;
                Vector4 Bw = second_half ? p1 + (p2 - p1) * beta : p0 + (p1 - p0) * beta;

                if ((int)Math.Ceiling(A.X) > (int)Math.Ceiling(B.X))
                {
                    SwapPoints(ref A, ref B);
                    SwapPoints(ref An, ref Bn);
                    SwapPoints(ref Aw, ref Bw);
                }

                int leftBorder = (int)Math.Ceiling(A.X);
                int rightBordet = (int)Math.Ceiling(B.X);

                for (int j = leftBorder; j < rightBordet; j++)
                {
                    int Ax = (int)Math.Ceiling(A.X);
                    int Bx = (int)Math.Ceiling(B.X);
                    float phi = Bx == Ax ? 1f : (j - Ax) / (float)(Bx - Ax);

                    Vector4 P = A + (B - A) * phi;
                    Vector4 pixelNormalVector = An + (Bn - An) * phi;
                    Vector4 pixelWorldPosition = Aw + (Bw - Aw) * phi;

                    Vector3 light_dir = Vector3.Normalize(new Vector3(pixelWorldPosition.X, pixelWorldPosition.Y, pixelWorldPosition.Z) - lightPosition);
                    Vector3 camera_dir = Vector3.Normalize(new Vector3(pixelWorldPosition.X, pixelWorldPosition.Y, pixelWorldPosition.Z) - Mapper.VertexToVector3(camera.Position));

                    Vector3 normalVector = Vector3.Normalize(new Vector3(pixelNormalVector.X, pixelNormalVector.Y, pixelNormalVector.Z));

                    var myColor = VertexColorByPhong(
                            normalVector,
                            light_dir,
                            camera_dir,
                            Color.FromArgb(50, 50, 50),
                            Color.Red,
                            Color.White,
                            Color.Black);

                    Color color = Color.FromArgb((int)Math.Min(myColor.X, 255), (int)Math.Min(myColor.Y, 255), (int)Math.Min(myColor.Z, 255));

                    int x = (int)Math.Ceiling(P.X);
                    int y = (int)Math.Ceiling(P.Y);
                    if (y > height || x > width || y < 0 || x < 0)
                    {
                        continue;
                    }

                    int index = y * width + x;
                    if (index >= zBuffer.Length)
                    {
                        continue;
                    }

                    if (zBuffer[index] > P.Z)
                    {
                        zBuffer[index] = P.Z;
                        source[index] = color.ToArgb();
                    }
                }
            }
        }

        private static void Swap(ref Vertex a, ref Vertex b) => (a, b) = (b, a);

        private static Vector3 VertexColorByPhong(
            Vector3 vertexNormal, 
            Vector3 lightDirection, 
            Vector3 cameraDirection,
            Color ambient,
            Color diffuse,
            Color reflect,
            Color emission,
            double diffuseCoef = 1,
            double reflectCoef = 0.5)
        {
            var ambientLightColor = new Vector3(ambient.R, ambient.G, ambient.B);
            var diffuseColor = new Vector3(diffuse.R, diffuse.G, diffuse.B);
            var specularColor = new Vector3(reflect.R, reflect.G, reflect.B);
            var emissionVector = new Vector3(emission.R, emission.G, emission.B);

            Vector3 L = lightDirection;
            Vector3 N = vertexNormal;
            
            float lambertComponent = (float)diffuseCoef * Math.Max(Vector3.Dot(N, L), 0);
            Vector3 diffuseLight = diffuseColor * lambertComponent;

            Vector3 V = Vector3.Normalize(cameraDirection);

            Vector3 R = L - 2 * Vector3.Dot(L, N) * N;

            float specular = (float)reflectCoef * (float)Math.Pow(Math.Max(Vector3.Dot(V, R), 0), shiness);
            Vector3 specularLight = specularColor * specular;

            Vector3 sumColor = ambientLightColor + diffuseLight + specularLight + emissionVector;

            return sumColor;
        }

        private static unsafe void DrawTexturedTriangle(
                Vector4[] modelVertices,
                Vector4[] worldVertices,
                Vector4[] worldNormalVectors,
                Vector4[] textures,
                int[,] diffuseMap,
                int[,] reflectMap,
                int[,] emissionMap,
                Vector3[,] normalsMap,
                int* source,
                int width,
                int height,
                double[] zBuffer,
                Camera camera,
                Vector3 lightPosition)
        {
            Vector4 v0 = modelVertices[0];// new Vector4(modelVertices[polygon.VertexIndices[0] - 1]);
            Vector4 v1 = modelVertices[1];// new Vector4(modelVertices[polygon.VertexIndices[1] - 1]);
            Vector4 v2 = modelVertices[2];// new Vector4(modelVertices[polygon.VertexIndices[2] - 1]);

            Vector4 p0 = worldVertices[0];//new Vector4(worldVertices[polygon.VertexIndices[0] - 1]);
            Vector4 p1 = worldVertices[1];
            Vector4 p2 = worldVertices[2];

            Vector4 n0 = worldNormalVectors[0];// new Vector4(worldNormalVectors[polygon.NormalIndices[0] - 1]);
            Vector4 n1 = worldNormalVectors[1];// new Vector4(worldNormalVectors[polygon.NormalIndices[1] - 1]);
            Vector4 n2 = worldNormalVectors[2];// new Vector4(worldNormalVectors[polygon.NormalIndices[2] - 1]);

            Vector4 t0 = textures[0];
            Vector4 t1 = textures[1];
            Vector4 t2 = textures[2];

            int mapWidth = diffuseMap.GetUpperBound(1) + 1;
            int mapHeight = diffuseMap.GetUpperBound(0) + 1;

            if ((int)Math.Ceiling(v0.Y) > (int)Math.Ceiling(v1.Y))
            {
                SwapPoints(ref v0, ref v1);
                SwapPoints(ref n0, ref n1);
                SwapPoints(ref p0, ref p1);
                SwapPoints(ref t0, ref t1);
            }

            if ((int)Math.Ceiling(v1.Y) > (int)Math.Ceiling(v2.Y))
            {
                SwapPoints(ref v1, ref v2);
                SwapPoints(ref n1, ref n2);
                SwapPoints(ref p1, ref p2);
                SwapPoints(ref t1, ref t2);
            }


            if ((int)Math.Ceiling(v0.Y) > (int)Math.Ceiling(v1.Y))
            {
                SwapPoints(ref v0, ref v1);
                SwapPoints(ref n0, ref n1);
                SwapPoints(ref p0, ref p1);
                SwapPoints(ref t0, ref t1);
            }

            int p0y = (int)Math.Ceiling(v0.Y);
            int p1y = (int)Math.Ceiling(v1.Y);
            int p2y = (int)Math.Ceiling(v2.Y);

            int total_height = p2y - p0y;

            for (int i = 0; i < total_height; i++)
            {
                bool second_half = i > p1y - p0y || p1y == p0y;
                int segment_height = second_half ? p2y - p1y : p1y - p0y;
                if (segment_height == 0)
                {
                    continue;
                }

                float alpha = (float)i / total_height;
                float beta = (float)(i - (second_half ? p1y - p0y : 0)) / segment_height;

                Vector4 A = v0 + (v2 - v0) * alpha;
                Vector4 B = second_half ? v1 + (v2 - v1) * beta : v0 + (v1 - v0) * beta;

                Vector4 An = n0 + (n2 - n0) * alpha;
                Vector4 Bn = second_half ? n1 + (n2 - n1) * beta : n0 + (n1 - n0) * beta;

                Vector4 Aw = p0 + (p2 - p0) * alpha;
                Vector4 Bw = second_half ? p1 + (p2 - p1) * beta : p0 + (p1 - p0) * beta;

                Vector4 At = ((1 - alpha) * t0 * v0.W + alpha * t2 * v2.W) / ((1 - alpha) * v0.W + alpha * v2.W);
                Vector4 Bt = second_half
                    ? ((1 - beta) * t1 * v1.W + beta * t2 * v2.W) / ((1 - beta) * v1.W + beta * v2.W)
                    : ((1 - beta) * t0 * v0.W + beta * t1 * v1.W) / ((1 - beta) * v0.W + beta * v1.W);

                if ((int)Math.Ceiling(A.X) > (int)Math.Ceiling(B.X))
                {
                    SwapPoints(ref A, ref B);
                    SwapPoints(ref An, ref Bn);
                    SwapPoints(ref Aw, ref Bw);
                    SwapPoints(ref At, ref Bt);
                }

                int leftBorder = (int)Math.Ceiling(A.X);
                int rightBordet = (int)Math.Ceiling(B.X);

                for (int j = leftBorder; j < rightBordet; j++)
                {
                    int Ax = (int)Math.Ceiling(A.X);
                    int Bx = (int)Math.Ceiling(B.X);
                    float phi = Bx == Ax ? 1f : (j - Ax) / (float)(Bx - Ax);

                    Vector4 P = A + (B - A) * phi;
                    Vector4 pixelNormalVector = An + (Bn - An) * phi;
                    Vector4 pixelWorldPosition = Aw + (Bw - Aw) * phi;

                    float lerpW = (1 - phi) * A.W + phi * B.W;
                    Vector4 pixelTexture = ((1 - phi) * (At * A.W) + phi * Bt * B.W) / lerpW;

                    int mapX = (int)(pixelTexture.X * (mapWidth - 1));
                    int mapY = (int)((1 - pixelTexture.Y) * (mapHeight - 1));

                    Vector3 light_dir = Vector3.Normalize(new Vector3(pixelWorldPosition.X, pixelWorldPosition.Y, pixelWorldPosition.Z) - lightPosition);
                    Vector3 camera_dir = Vector3.Normalize(new Vector3(pixelWorldPosition.X, pixelWorldPosition.Y, pixelWorldPosition.Z) - Mapper.VertexToVector3(camera.Position));

                    Vector3 normalVector = Vector3.Normalize(new Vector3(pixelNormalVector.X, pixelNormalVector.Y, pixelNormalVector.Z));
                    Vector3 normalVector2 = normalsMap[mapY, mapX];

                    Color diffuse = Color.FromArgb(diffuseMap[mapY, mapX]);
                    Color reflect = Color.FromArgb(reflectMap[mapY, mapX]);
                    Color emission = Color.FromArgb(emissionMap[mapY, mapX]);

                    var myColor = VertexColorByPhong(
                            normalVector2,
                            light_dir,
                            camera_dir,
                            Color.FromArgb(0, 0, 0),
                            diffuse,
                            reflect,
                            emission);

                    Color color = Color.FromArgb((int)Math.Min(myColor.X, 255), (int)Math.Min(myColor.Y, 255), (int)Math.Min(myColor.Z, 255));

                    int x = (int)Math.Ceiling(P.X);
                    int y = (int)Math.Ceiling(P.Y);
                    if (y > height || x > width || y < 0 || x < 0)
                    {
                        continue;
                    }

                    int index = y * width + x;
                    if (index >= zBuffer.Length)
                    {
                        continue;
                    }

                    if (zBuffer[index] > P.Z)
                    {
                        zBuffer[index] = P.Z;
                        source[index] = color.ToArgb();

                        if (GetBrightness(emission.ToArgb()) > 100)
                        {
                            bloomBuffer[index] = color.ToArgb();
                        }
                        else
                        {
                            bloomBuffer[index] = 0;
                        }
                    }
                }
            }
        }
    }
}
