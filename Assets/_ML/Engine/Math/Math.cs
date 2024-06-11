using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.MathNS
{
    public static class Math
    {
        public static int GetRandomIndex(List<int> weights)
        {
            int totalWeight = 0;
            foreach (int weight in weights)
            {
                totalWeight += weight;
            }
            int num = UnityEngine.Random.Range(0, totalWeight + 1);
            int cur = 0;
            for (int i = 0; i < weights.Count; i++)
            {
                cur += weights[i];
                if (num <= cur)
                {
                    return i;
                }
            }
            return 0;
        }

        public static double GetNegSkewedDistribution(double mean, double var)
        {
            if (var > 0)
            {
                double u1 = UnityEngine.Random.Range(0f, 1f);
                double u2 = UnityEngine.Random.Range(0f, 1f);
                double normalRandom = System.Math.Sqrt(-2.0 * System.Math.Log(u1)) * System.Math.Sin(2.0 * System.Math.PI * u2);
                return mean - normalRandom * System.Math.Sqrt(var);
            }
            return mean;
        }
        public static double GetNegSkewedDistribution(double low, double high, double mean, double var)
        {
            int cnt = 100;
            while (cnt > 0)
            {
                cnt--;
                double rand = GetNegSkewedDistribution(mean, var);
                if (low <= rand && rand <= high)
                {
                    return rand;
                }
            }
            return low;
        }
    }
}