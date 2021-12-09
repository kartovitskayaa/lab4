using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RendererConsole
{
    class Filters
    {
        private static Dictionary<int, double[]> kernels = new Dictionary<int, double[]>();

        public static double[] GetGaussKernel(int m)
        {
            int windowSize = 2 * m + 1;
            double sigma = m / 3;

            if (kernels.ContainsKey(m))
            {
                return kernels[m];
            }

            var result = new double[windowSize];

            for (int i = -m; i <= m; i++)
            {
                result[i + m] = Math.Exp(-i * i / (2 * sigma * sigma)) / (Math.Sqrt(2 * Math.PI) * sigma);
            }

            kernels.Add(m, result);

            return result;
        }

    }
}
