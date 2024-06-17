using System.Collections.Generic;

namespace ML.Engine.MathNS
{
    public static class Math
    {
        private static System.Random Rand = new System.Random();
        public static int GetRandomIndex(int size)
        {
            return size > 0 ? Rand.Next(0, size) : 0;
        }
        public static List<int> GetRandomIndex(int size, int n, bool back = true)
        {
            List<int> result = new List<int>();
            if (size > 0 && n > 0)
            {
                if (back)
                {
                    for (int i = 0; i < n; i++)
                    {
                        result.Add(GetRandomIndex(size));
                    }
                }
                else
                {
                    List<int> weights = new List<int>();
                    for (int i = 0; i < size; i++)
                    {
                        weights[i] = 1;
                    }
                    for (int i = 0; i < n; i++)
                    {
                        int index = GetRandomIndex(weights);
                        result.Add(index);
                        weights[index] = 0;
                    }
                }
            }
            return result;
        }
        public static int GetRandomIndex(List<int> weights)
        {
            if (weights != null && weights.Count > 0)
            {
                int totalWeight = 0;
                foreach (int weight in weights)
                {
                    totalWeight += weight;
                }
                int num = Rand.Next(0, System.Math.Max(totalWeight, 1));
                int cur = 0;
                for (int i = 0; i < weights.Count; i++)
                {
                    cur += weights[i];
                    if (num <= cur)
                    {
                        return i;
                    }
                }
            }
            return 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="back">是否放回</param>
        public static List<int> GetRandomIndex(List<int> weights, int n, bool back=true)
        {
            List<int> result = new List<int>();
            if (weights != null && weights.Count > 0 && n > 0)
            {
                if (back)
                {
                    for (int i = 0; i < n; i++)
                    {
                        result.Add(GetRandomIndex(weights));
                    }
                }
                else
                {
                    List<int> weightsCopy = new List<int>(weights);
                    for (int i = 0; i < n; i++)
                    {
                        int index = GetRandomIndex(weightsCopy);
                        result.Add(index);
                        weightsCopy[index] = 0;
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// [low, high]
        /// </summary>
        public static int UniformDistribution(int low, int high)
        {
            return Rand.Next(low, high + 1);
        }
        public static double UniformDistribution(double low, double high, int round=3)
        {
            double result = low + (high - low) * Rand.NextDouble();
            return System.Math.Round(result, round);
        }
        /// <summary>
        /// 正态分布随机数生成
        /// </summary>
        /// <param name="mu">均值</param>
        /// <param name="sigma2">方差</param>
        public static double NormalDistribution(double mu, double sigma2, int round = 3)
        {
            double r1 = Rand.NextDouble();
            double r2 = Rand.NextDouble();
            double r = System.Math.Sqrt((-2) * System.Math.Log(r2)) * System.Math.Sin(2 * System.Math.PI * r1);
            double result = mu + sigma2 * r;
            return System.Math.Round(result, round);
        }
        public static double NormalDistribution(double low, double high, double mu, double sigma2, int round = 3)
        {
            while (true)
            {
                double rand = NormalDistribution(mu, sigma2, round);
                if (low <= rand && rand <= high)
                {
                    return rand;
                }
            }
        }
        public static double SkewedDistribution(double mu, double sigma2, double lambdaParam, int round = 3)
        {
            double alpha = lambdaParam;
            double loc = mu;
            double scale = System.Math.Sqrt(sigma2);
            double sigma = alpha / System.Math.Sqrt(1.0 + alpha * alpha);

            double u1 = Rand.NextDouble();
            double u2 = Rand.NextDouble();
            double u0 = System.Math.Sqrt(-2.0 * System.Math.Log(u1)) * System.Math.Cos(2.0 * System.Math.PI * u2);

            u1 = Rand.NextDouble();
            u2 = Rand.NextDouble();
            double v = System.Math.Sqrt(-2.0 * System.Math.Log(u1)) * System.Math.Cos(2.0 * System.Math.PI * u2);
            u1 = (sigma * u0 + System.Math.Sqrt(1.0 - sigma * sigma) * v) * scale;
            if (u0 < 0) { u1 *= -1; }
            return System.Math.Round(u1 + loc, round);
        }
        public static double SkewedDistribution(double low, double high, double mu, double sigma2, double lambdaParam, int round = 3)
        {
            while (true)
            {
                double rand = SkewedDistribution(mu, sigma2, lambdaParam, round);
                if (low <= rand && rand <= high)
                {
                    return rand;
                }
            }
        }
    }
}