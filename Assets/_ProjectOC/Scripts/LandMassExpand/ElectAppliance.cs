using ML.Engine.BuildingSystem.BuildingPart;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.LandMassExpand
{
    public class ElectAppliance : BuildingPart, INeedPowerBpart
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
                    InPower = false;
                }
                else
                {
                    InPower = true;
                }
            }
        }
        
        private bool inPower = false;

        [ShowInInspector]public bool InPower
        {
            get => inPower;
            set
            {
                inPower = value;
                if(vfxTranf != null)
                    vfxTranf.SetActive(value);
                
                //bug to-do
                if (value)
                    powerVFXMat.SetColor("_Color",inPowerColor);
                else
                    powerVFXMat.SetColor("_Color",unPowerColor);
            }
        }

        [SerializeField] private Color inPowerColor, unPowerColor;
        private Material powerVFXMat;
        [SerializeField]private GameObject vfxTranf;
        private new void Awake()
        {
            powerVFXMat = GetComponent<Renderer>().material;
            GetComponent<Renderer>().sharedMaterial = powerVFXMat;

            base.Awake();
        }
        void OnDestroy()
        {
            if (BuildPowerIslandManager.Instance != null && InPower && BuildPowerIslandManager.Instance.electAppliances.Contains(this))
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
                {
                    if(powerSub.InPower && BuildPowerIslandManager.Instance.CoverEachOther(0, powerSub.PowerSupportRange, 
                           transform.position, powerSub.transform.position))
                        tempCount++;                    
                }
                
            }

            PowerCount = tempCount;
        }
        
        
    }
}