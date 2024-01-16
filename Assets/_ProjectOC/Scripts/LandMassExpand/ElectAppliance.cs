using ML.Engine.BuildingSystem.BuildingPart;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.LandMassExpand
{
    public class ElectAppliance : BuildingPart, IPowerBPart
    {
        [Header("用电器部分")] private int powerCount = 0;

        [ShowInInspector]public int PowerCount
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

        private int powerSupportRange;

        [ShowInInspector]public int PowerSupportRange
        {
            get => powerSupportRange;
            set
            {
                powerSupportRange = value;
            }
        }

        private bool inpower = false;

        [ShowInInspector]public bool Inpower
        {
            get => inpower;
            set
            {
                inpower = value;
                
                if (inpower)
                    powerVFXMat.SetColor("_Color",inPowerColor);
                else
                    powerVFXMat.SetColor("_Color",unPowerColor);
                //Debug.Log("Color: " + powerVFXMat.GetColor("_Color"));
            }
        }

        [SerializeField] private Color inPowerColor, unPowerColor;
        private Material powerVFXMat;
        private new void Awake()
        {
            powerVFXMat = GetComponent<Renderer>().material;
            GetComponent<Renderer>().sharedMaterial = powerVFXMat;

            base.Awake();
        }
        void OnDestroy()
        {
            if (BuildPowerIslandManager.Instance != null && Inpower && BuildPowerIslandManager.Instance.electAppliances.Contains(this))
            {
                BuildPowerIslandManager.Instance.electAppliances.Remove(this);
            }
        }

        public new void OnChangePlaceEvent(Vector3 oldPos, Vector3 newPos)
        {
            //如果没有，说明刚建造则加入
            if (!BuildPowerIslandManager.Instance.electAppliances.Contains(this))
            {
                BuildPowerIslandManager.Instance.electAppliances.Add(this);
            }
            RecalculatePowerCount();
        }

        private void RecalculatePowerCount()
        {
            int tempCount = 0;
            foreach (var powerCore in BuildPowerIslandManager.Instance.powerCores)
            {
                if(BuildPowerIslandManager.Instance.CoverEachOther(0, powerCore.PowerSupportRange, 
                       transform.position, powerCore.transform.position))
                    tempCount++;
            }
            
            foreach (var powerSub in BuildPowerIslandManager.Instance.powerSubs)
            {
                if(BuildPowerIslandManager.Instance.CoverEachOther(0, powerSub.PowerSupportRange, 
                       transform.position, powerSub.transform.position))
                    tempCount++;
            }

            PowerCount = tempCount;
        }
        
        
    }
}