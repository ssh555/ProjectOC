using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    public class PinchPartDataManager : MonoBehaviour
    {
        public Dictionary<PinchPartType3, PinchPartType> pinchPartType;
        
        
        public static float RemapValue(float value, float fromMin, float fromMax, float toMin = 0f, float toMax = 1f)
        {
            // 将 value 从 fromMin 到 fromMax 范围映射到 toMin 到 toMax 范围
            return toMin + (value - fromMin) / (fromMax - fromMin) * (toMax - toMin);
        }   
    }
}
