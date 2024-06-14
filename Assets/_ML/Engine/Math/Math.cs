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
    }
}