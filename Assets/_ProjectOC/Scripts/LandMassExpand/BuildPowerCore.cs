using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.BuildingSystem
{
    public class BuildPowerCore : MonoBehaviour
    {
        public int powerSupportRange;

        //��Ȧ��Ч
        [SerializeField] private Transform powerSupportVisual;

        //�ɽ��췶Χ
        [SerializeField] int buildableRange;
    }
}
