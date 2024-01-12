using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.BuildingSystem
{
    public class BuildPowerCore : MonoBehaviour
    {
        public int powerSupportRange;

        //光圈特效
        [SerializeField] private Transform powerSupportVisual;

        //可建造范围
        [SerializeField] int buildableRange;
    }
}
