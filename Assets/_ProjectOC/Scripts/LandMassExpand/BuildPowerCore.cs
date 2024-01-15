using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ML.Engine.BuildingSystem
{
    public class BuildPowerCore : MonoBehaviour
    {
        [SerializeField] private Transform powerSupportVFX;
        
        private int powerSupportRange = 20;
        [ShowInInspector]public int PowerSupportRange
        {
            get => powerSupportRange;
            set
            {
                powerSupportRange = value;
                float _localScale = value * 0.2f;
                powerSupportVFX.transform.localScale = new Vector3(_localScale,1,_localScale);
            }
        }

        private void Awake()
        {
            BuildPowerIslandManager.Instance.powerCores.Add(this);
        }
        void OnDestroy()
        {
            BuildPowerIslandManager.Instance.powerCores.Remove(this);
        }

    }
}
