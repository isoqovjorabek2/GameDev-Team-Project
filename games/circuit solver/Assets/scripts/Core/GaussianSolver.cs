using System;

namespace CircuitSolver.Core
{
    /// <summary>
    /// Dense Gaussian elimination with partial pivoting. Used by the MNA
    /// solver to invert the [A]{x} = {z} system that arises from KCL/KVL.
    /// </summary>
    public static class GaussianSolver
    {
        /// <summary>
        /// Solves A*x = b in place. Returns true if the system has a unique
        /// solution (non-singular). Writes the answer into the last column
        /// during elimination; caller receives a fresh x array.
        /// </summary>
        public static bool Solve(double[,] A, double[] b, out double[] x, double singularEps = 1e-12)
        {
            int n = b.Length;
            if (A.GetLength(0) != n || A.GetLength(1) != n)
                throw new ArgumentException("Gaussian solver requires a square matrix matching b.");

            var M = new double[n, n + 1];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++) M[i, j] = A[i, j];
                M[i, n] = b[i];
            }

            for (int k = 0; k < n; k++)
            {
                int pivot = k;
                double maxAbs = Math.Abs(M[k, k]);
                for (int i = k + 1; i < n; i++)
                {
                    double a = Math.Abs(M[i, k]);
                    if (a > maxAbs) { maxAbs = a; pivot = i; }
                }

                if (maxAbs < singularEps)
                {
                    x = null;
                    return false;
                }

                if (pivot != k)
                {
                    for (int j = k; j <= n; j++)
                    {
                        double tmp = M[k, j];
                        M[k, j] = M[pivot, j];
                        M[pivot, j] = tmp;
                    }
                }

                for (int i = k + 1; i < n; i++)
                {
                    double factor = M[i, k] / M[k, k];
                    if (factor == 0d) continue;
                    for (int j = k; j <= n; j++)
                        M[i, j] -= factor * M[k, j];
                }
            }

            x = new double[n];
            for (int i = n - 1; i >= 0; i--)
            {
                double sum = M[i, n];
                for (int j = i + 1; j < n; j++) sum -= M[i, j] * x[j];
                x[i] = sum / M[i, i];
            }
            return true;
        }
    }
}
