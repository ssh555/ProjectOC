using System.Collections;
using System.Collections.Generic;
using ML.Engine.BuildingSystem.BuildingPart;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ML.Engine.BuildingSystem.BuildingPart
{
    public class ElectAppliance : BuildingPart, IPowerBPart
    {
        [Header("用电器部分")] private int powerCount;

        public int PowerCount
        {
            get => powerCount;
            set
            {
                powerCount = value;
                if (powerCount <= 0)
                {
                    Inpower = false;
                }
                else
                {
                    Inpower = true;
                }
            }
        }

        private bool inpower = false;

        [ShowInInspector]public bool Inpower
        {
            get => inpower;
            set
            {
                inpower = value;
                Debug.Log("Color: " + powerVFXMat.GetColor("_Color"));
                
                if (inpower)
                    powerVFXMat.SetColor("_Color",inPowerColor);
                else
                    powerVFXMat.SetColor("_Color",unPowerColor);
            }
        }

        [SerializeField] private Color inPowerColor, unPowerColor;
        private Material powerVFXMat;
        private new void Awake()
        {
            BuildPowerIslandManager.Instance.electAppliances.Add(this);
            powerVFXMat = GetComponent<Renderer>().material;
            GetComponent<Renderer>().sharedMaterial = powerVFXMat;

            base.Awake();
        }
        void OnDestroy()
        {
            BuildPowerIslandManager.Instance.electAppliances.Remove(this);
        }

        public new void OnChangePlaceEvent(Vector3 oldPos, Vector3 newPos)
        {
            RecalculatePowerCount();
        }

        private void RecalculatePowerCount()
        {
            int tempCount = 0;
            foreach (var powerCore in BuildPowerIslandManager.Instance.powerCores)
            {
                if(BuildPowerIslandManager.Instance.RangeCoverEachOther(0, powerCore.PowerSupportRange, 
                       transform.position, powerCore.transform.position))
                    tempCount++;
            }
            
            foreach (var powerSub in BuildPowerIslandManager.Instance.powerSubs)
            {
                if(BuildPowerIslandManager.Instance.RangeCoverEachOther(0, powerSub.PowerSupportRange, 
                       transform.position, powerSub.transform.position))
                    tempCount++;
            }

            PowerCount = tempCount;
        }
        
        
    }
}