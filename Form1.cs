using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Numerics;
using Smoothing;

namespace RendererConsole
{
    public partial class Form1 : Form
    {
        private const string DiffuseMapFile = "Albedo Map.png";
        private const string NormalMapFile = "Normal Map.png";
        private const string SpecularMapFile = "Specular Map.png";
        private const string EmissionMapFile = "Emission Map.png";

        private Vertex[] vertices;
        private Vertex[] normals;
        private Vertex[] textures;
        private int[] indices;
        private int[] normalsIndices;
        private int[] texturesIndices;

        private int[,] diffuseMap;
        private int[,] normalsMapData;
        private int[,] emissionMap;
        private Vector3[,] normalsMap;
        private int[,] specularMap;

        private Camera camera;
        private const double speed = 5;
        private const double ligthSpeed = 10;

        private double alpha;
        private double betta;
        private double scale = 1;

        private Vertex directionalLight = new Vertex(10, 10, -10);
        private Vertex pointLight = new Vertex(0, 0, -10);

        public Form1()
        {
            this.camera = new Camera()
            {
                Direction = new Vertex(0, 0, -1),
                Position = new Vertex(0, 0, 4),
                Up = new Vertex(0, 1, 0),
            };

            InitializeComponent();
            this.MouseWheel += this.OnMousWheel;
        }

        private void btDraw_Click(object sender, EventArgs e)
        {
            Draw();
        }

        private void Draw()
        {            
            var bitmap = new Bitmap(pbCanvas.Width, pbCanvas.Height, PixelFormat.Format32bppArgb);

            var polygons = new List<Point[]>();
            var polygons3d = new List<Vertex[]>();
            var worldPolygons3d = new List<Vertex[]>();
            var facesNormals = new List<Vertex[]>();
            var normals = new List<Vertex>();
            var facesTextures = new List<Vertex[]>();

            int width = this.pbCanvas.Width;
            int height = this.pbCanvas.Height;

            if (!(this.vertices is null || this.indices is null))
            {
                int tmStart = Environment.TickCount;
                double[,] rotationMatrix = MatrixHelper.GetRotationY(alpha)
                    .Multiply(MatrixHelper.GetRotationX(betta));

                double[,] modelMatrix = rotationMatrix
                    .Multiply(MatrixHelper.GetScaleMatrix(scale, scale, scale));

                double[,] viewMatrix = this.camera.GetLookAtMatrix();

                double[,] projectionMatrix = MatrixHelper.GetProjection(width / (double)height, Math.PI / 3, 0.2, 15);

                double[,] viewportMatrix = MatrixHelper.GetViewport(0, 0, width, height);

                double[,] mvp = projectionMatrix.Multiply(viewMatrix).Multiply(modelMatrix);

                var worldVertices = vertices
                    .Select(v => Vector4.Transform(Mapper.VertexToVector4(v), Mapper.MatrixToMatrix4x4(modelMatrix)))
                    .Select(v => Mapper.Vector4ToVertex(new Vector4(v.X, v.Y, v.Z, v.W)))
                    .ToArray();

                var finalVertices = vertices.Select(v => Vector4.Transform(Mapper.VertexToVector4(v), Mapper.MatrixToMatrix4x4(modelMatrix)))
                   .Select(v => Vector4.Transform(v, Mapper.MatrixToMatrix4x4(viewMatrix)))
                   .Select(v => Vector4.Transform(v, Mapper.MatrixToMatrix4x4(projectionMatrix)))
                   .Select(v => Mapper.Vector4ToVertex(new Vector4(v.X, v.Y, v.Z, v.W)))
                   .ToArray();

                var rotatedNormals = this.normals
                    .Select(v => Vector4.Transform(Mapper.VertexToVector4(v), Mapper.MatrixToMatrix4x4(modelMatrix)))
                   .Select(v => Mapper.Vector4ToVertex(new Vector4(v.X, v.Y, v.Z, v.W)).Normalize())
                   .ToArray();

                for (int i = 0; i <= this.normalsMap.GetUpperBound(0); i++)
                {
                    for(int j = 0; j <= this.normalsMap.GetUpperBound(1); j++)
                    {
                        Color tempColor = Color.FromArgb(normalsMapData[i, j]);

                        Vector3 normal = new Vector3(tempColor.R, tempColor.G, tempColor.B) * 2 - new Vector3(255, 255, 255);
                        normal = Vector3.Normalize(normal);

                        normal = Vector3.TransformNormal(normal, Mapper.MatrixToMatrix4x4(rotationMatrix));
                        normalsMap[i, j] = normal;
                    }
                }

                int border = this.indices.Length / 3;

                double[] tempW = new double[finalVertices.Length];
                for (int i = 0; i < finalVertices.Length; i++)
                {
                    finalVertices[i].X /= finalVertices[i].W;
                    finalVertices[i].Y /= finalVertices[i].W;
                    finalVertices[i].Z /= finalVertices[i].W;
                    tempW[i] = 1 / finalVertices[i].W;
                }


                finalVertices = finalVertices.Select(v => Vector4.Transform(Mapper.VertexToVector4(v), Mapper.MatrixToMatrix4x4(viewportMatrix)))
                   .Select((v, i) => new Vertex(v.X, v.Y, v.Z, tempW[i]))
                   .ToArray();

                for (int i = 0; i < border; i++)
                {
                    Vertex[] polygon3D;
                    Vertex[] worldPolygon3D;
                    Vertex[] triangleNormals;
                    Vertex[] triangleTextures;

                    try
                    {
                        polygon3D = new Vertex[]
                        {
                            finalVertices[indices[3 * i] - 1],
                            finalVertices[indices[3 * i + 1] - 1],
                            finalVertices[indices[3 * i + 2] - 1],
                        };

                        worldPolygon3D = new Vertex[]
                        {
                            worldVertices[indices[3 * i] - 1],
                            worldVertices[indices[3 * i + 1] - 1],
                            worldVertices[indices[3 * i + 2] - 1],
                        };

                        triangleNormals = new Vertex[]
                        {
                            rotatedNormals[normalsIndices[3 * i] - 1],
                            rotatedNormals[normalsIndices[3 * i + 1] - 1],
                            rotatedNormals[normalsIndices[3 * i + 2] - 1],
                        };


                        triangleTextures = new Vertex[]
                        {
                            this.textures[texturesIndices[3 * i] - 1],
                            this.textures[texturesIndices[3 * i + 1] - 1],
                            this.textures[texturesIndices[3 * i + 2] - 1],
                        };
                    }
                    catch (IndexOutOfRangeException)
                    {
                        break;
                    }

                    facesNormals.Add(triangleNormals);
                    polygons3d.Add(polygon3D);
                    worldPolygons3d.Add(worldPolygon3D);
                    facesTextures.Add(triangleTextures);
                }

                int tmLambertStart = Environment.TickCount;
                //    bitmap = Drawer.DrawPolygons(bitmap, polygons3d.Select(p => p.Select(v => new Point((int)v.X, (int)v.Y)).ToArray()));
                bitmap = Drawer.FillTexturedTriangle(
                    bitmap, 
                    camera, 
                    pointLight, 
                    polygons3d.ToArray(), 
                    worldPolygons3d.ToArray(), 
                    facesNormals.ToArray(), 
                    facesTextures.ToArray(), 
                    diffuseMap, 
                    specularMap, 
                    emissionMap,
                    normalsMap);
               // bitmap = Drawer.FillTrianglesPhong(bitmap, this.camera, this.pointLight, polygons3d.ToArray(), worldPolygons3d.ToArray(), facesNormals.ToArray(), Color.Red);
                int tmLambertEnd = Environment.TickCount;
                Console.WriteLine($"Lambert: {tmLambertEnd - tmLambertStart}");

                int tmEnd = Environment.TickCount;
                Console.WriteLine($"All: {tmEnd - tmStart}");
                pbCanvas.Image = bitmap;
            }
        }

        private void OnMousWheel(object sender, MouseEventArgs e)
        {
            this.scale += e.Delta / 1000.0;
        }

        private void btLoad_Click(object sender, EventArgs e)
        {
            LoadModel();
        }

        private void LoadModel()
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string filename = dialog.FileName;
                using (var reader = new StreamReader(File.OpenRead(filename)))
                {
                    var parser = new ObjParser();
                    parser.Parse(reader);

                    this.vertices = parser.Vertices?.ToArray();
                    this.indices = parser.Faces?.ToArray();
                    this.normals = parser.Normals?.ToArray();
                    this.normalsIndices = parser.FacesNormals?.ToArray();
                    this.textures = parser.Textures?.ToArray();
                    this.texturesIndices = parser.FacesTextures?.ToArray();
                    //     this.normals = parser.
                }

                filename = $"{Path.GetDirectoryName(filename)}\\{DiffuseMapFile}";
                this.diffuseMap = GetTexture(filename);

                filename = $"{Path.GetDirectoryName(filename)}\\{SpecularMapFile}";
                this.specularMap = GetTexture(filename);

                filename = $"{Path.GetDirectoryName(filename)}\\{NormalMapFile}";
                this.normalsMapData = GetTexture(filename);

                filename = $"{Path.GetDirectoryName(filename)}\\{EmissionMapFile}";
                this.emissionMap = GetTexture(filename);

                this.normalsMap = new Vector3[normalsMapData.GetUpperBound(0) + 1, normalsMapData.GetUpperBound(1) + 1];
            }
        }

        private static int[,] GetTexture(string filename)
        {
            int[,] result;
            using (var stream = File.OpenRead(filename))
            {
                var bitmap = new Bitmap(stream);

                result = bitmap.GetPixels();
            }

            return result;
        }

        private void btRight_Click(object sender, EventArgs e)
        {
            this.camera.Position = new Vertex(camera.Position.X + 1, camera.Position.Y, camera.Position.Z);
            // alpha += Delta;
        }

        private void btLeft_Click(object sender, EventArgs e)
        {
            this.camera.Direction = new Vertex(camera.Direction.X - 1, camera.Direction.Y, camera.Direction.Z);
            // alpha -= Delta;
        }

        private void onMouseWheel(object sender, MouseEventArgs e)
        {
            this.scale *= 1.1 * Math.Sign(e.Delta);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void tmUpdate_Tick(object sender, EventArgs e)
        {
            double elapsedTime = this.tmUpdate.Interval / 1000.0;
            double delta = elapsedTime * speed;

            if (KeyboardState.IsKeyDown((int)Keys.Left))
            {
                alpha -= delta;
            }

            if (KeyboardState.IsKeyDown((int)Keys.Right))
            {
                alpha += delta;
            }

            if (KeyboardState.IsKeyDown((int)Keys.Up))
            {
                betta += delta;
            }

            if (KeyboardState.IsKeyDown((int)Keys.Down))
            {
                betta -= delta;
            }

            if (KeyboardState.IsKeyDown((int)Keys.W))
            {
                this.pointLight.Y += elapsedTime * ligthSpeed; ;
            }

            if (KeyboardState.IsKeyDown((int)Keys.S))
            {
                this.pointLight.Y -= elapsedTime * ligthSpeed; ;
            }

            if (KeyboardState.IsKeyDown((int)Keys.A))
            {
                this.pointLight.X -= elapsedTime * ligthSpeed;
            }
            
            if (KeyboardState.IsKeyDown((int)Keys.D))
            {
                this.pointLight.X += elapsedTime * ligthSpeed;
            }


            lbX.Text = this.camera.Position.X.ToString();
            lbZ.Text = this.camera.Position.Z.ToString();

            Draw();
        }

        private void btInc_Click(object sender, EventArgs e)
        {
            this.scale += 0.05;
        }

        private void btDec_Click(object sender, EventArgs e)
        {
            this.scale -= 0.05;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadModel();
        }

        private static int Min(int a, int b) => a < b ? a : b;

        private void pbCanvas_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            this.pbCanvas.Height = this.Height;
            this.pbCanvas.Width = this.Width;
        }
    }
}
