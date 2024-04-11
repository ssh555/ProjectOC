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
            // �� value �� fromMin �� fromMax ��Χӳ�䵽 toMin �� toMax ��Χ
            return toMin + (value - fromMin) / (fromMax - fromMin) * (toMax - toMin);
        }   
    }
}
