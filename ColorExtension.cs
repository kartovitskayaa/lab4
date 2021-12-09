using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace RendererConsole
{
    public static class ColorExtension
    {
        public static Color Multiply(this Color color, double coeff)
        {
            return Color.FromArgb(
                (byte)(Math.Min(color.R * coeff, 255)),
                (byte)(Math.Min(color.G * coeff, 255)), (byte)(color.B * coeff));
        }

        public static Color Add(this Color fisrt, Color second)
        {
            return Color.FromArgb(
                (byte)Math.Min(fisrt.R + second.R, 255),
                (byte)Math.Min(fisrt.G + second.G, 255),
                (byte)Math.Min(fisrt.B + second.B, 255));
        }
    }
}
