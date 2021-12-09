using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;

namespace RendererConsole
{
    public class ObjParser
    {
        private readonly Regex regexVertex = new Regex(@"v\s+(?<x>[0-9,.\-]+)\s+(?<y>[0-9,.\-]+)\s+(?<z>[0-9,.\-]+)(\s+(?<w>[0-9,.\-]+))*");
        private readonly Regex regexFace = new Regex(@"f(\s+(?<v>[0-9]+)(\/(?<vt>[0-9]+)*\/(?<vn>[0-9]+)*)*)+");
        private readonly Regex regexNormal = new Regex(@"vn\s+(?<i>[0-9,.\-]+)\s+(?<j>[0-9,.\-]+)\s+(?<k>[0-9,.\-]+)");
        private readonly Regex regexTexture = new Regex(@"vt\s+(?<u>[0-9,.\-]+)\s+(?<v>[0-9,.\-]+)(\s+(?<w>[0-9,.\-]+))*");

        private IEnumerable<(Regex regex, Action<Match> handler)> regexHandlers;

        public ObjParser()
        {
            this.regexHandlers = new (Regex, Action<Match>)[]
            {
                (regexVertex, HandleVertex),
                (regexFace, HandleFace),
                (regexNormal, HandleNormal),
                (regexTexture, HandleTexture)
            };

        }

        private List<Vertex> vertices = new List<Vertex>();
        private List<Vertex> normals = new List<Vertex>();
        private List<Vertex> textures = new List<Vertex>();
        private List<int> faces = new List<int>();
        private List<int> facesNormals = new List<int>();
        private List<int> facesTextures = new List<int>();

        public IEnumerable<Vertex> Vertices { get => vertices; }

        public IEnumerable<int> Faces { get => faces; }

        public IEnumerable<Vertex> Normals { get => normals; }

        public IEnumerable<int> FacesNormals { get => facesNormals; }

        public IEnumerable<Vertex> Textures { get => textures; }

        public IEnumerable<int> FacesTextures { get => facesTextures; }

        public void Parse(TextReader reader)
        {
            this.vertices.Clear();
            this.faces.Clear();
            this.normals.Clear();
            this.facesNormals.Clear();

            string temp;
            while ((temp = reader.ReadLine()) != null)
            {
                Match match;
                foreach (var pair in regexHandlers)
                {
                    match = pair.regex.Match(temp);
                    if (match.Success)
                    {
                        pair.handler(match);
                        break;
                    }
                }
            }
        }

        private void HandleNormal(Match match)
        {
            double i, j, k;
            if (!double.TryParse(
                match.Groups["i"].Value,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out i))
            {
                return;
            }

            if (!double.TryParse(
                match.Groups["j"].Value,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out j))
            {
                return;
            }

            if (!double.TryParse(
                match.Groups["k"].Value,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out k))
            {
                return;
            }

            this.normals.Add(new Vertex(i, j, k));
        }

        private void HandleTexture(Match match)
        {
            double v, u;
            if (!double.TryParse(
                match.Groups["v"].Value,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out v))
            {
                return;
            }

            if (!double.TryParse(
                match.Groups["u"].Value,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out u))
            {
                return;
            }

            this.textures.Add(new Vertex(u, v, 0));
        }

        private void HandleVertex(Match match)
        {
            double x, y, z;
            if (!double.TryParse(
                match.Groups["x"].Value,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out x))
            {
                return;
            }

            if (!double.TryParse(
                match.Groups["y"].Value,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out y))
            {
                return;
            }

            if (!double.TryParse(
                match.Groups["z"].Value,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out z))
            {
                return;
            }

            this.vertices.Add(new Vertex(x, y, z));
        }

        private void HandleFace(Match match)
        { 
            foreach (dynamic temp in match.Groups["v"].Captures)
            {
                HandleCapture(temp);
            }

            foreach (dynamic temp in match.Groups["vn"].Captures)
            {
                int index;
                string strIndex = temp.Value;
                if (int.TryParse(strIndex, out index))
                {
                    this.facesNormals.Add(index);
                }
            }

            foreach (dynamic temp in match.Groups["vt"].Captures)
            {
                int index;
                string strIndex = temp.Value;
                if (int.TryParse(strIndex, out index))
                {
                    this.facesTextures.Add(index);
                }
            }
        }

        private void HandleCapture(Group group)
        {
            int index;
            string strIndex = group.Value;
            if (int.TryParse(strIndex, out index))
            {
                this.faces.Add(index);
            }
        }

        private void HandleCapture(Capture capture)
        {
            int index;
            string strIndex = capture.Value;
            if (int.TryParse(strIndex, out index))
            {
                this.faces.Add(index);
            }
        }
    }
}
