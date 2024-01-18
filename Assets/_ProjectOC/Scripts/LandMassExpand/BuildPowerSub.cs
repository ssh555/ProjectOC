using ML.Engine.InventorySystem.CompositeSystem;
using ML.Engine.BuildingSystem.BuildingPart;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.LandMassExpand
{
    //拷贝自BuildingPart，增加OnChangePlaceEvent，放置后更新用电数量

    public class BuildPowerSub : BuildingPart, ISupportPowerBPart, IComposition
    {
        [Header("供电部分"),SerializeField,LabelText("供电范围")]
        private float powerSupportRange = 5;
        public float PowerSupportRange
        {
            get => powerSupportRange;
            set
            {
                powerSupportRange = value;
                float localScale = value * 0.2f;
                powerSupportVFX.transform.localScale = new Vector3(localScale,1,localScale);
            }
        }
        
        [SerializeField,LabelText("供电范围特效")]
        private GameObject powerSupportVFX;
        [SerializeField]
        private Material powerVFXMat;

        #region IPowerBPart

        private int powerCount;
        public int PowerCount 
        { 
            get=>powerCount;
            set => powerCount = value;
        }
  
        private bool inPower = false;
        [ShowInInspector]
        public bool InPower
        {
            get=>this.inPower;
            set
            {
                if (value != inPower)
                {
                    inPower = value;
                    //更换颜色
                    Color32 vfxColor = (value ? new Color32(255, 178,126,255): new Color32(139, 167,236,255));
                    powerVFXMat.SetColor("_VFXColor",vfxColor);

                    //在位置不变的情况下，计算附近用电器powercount情况，
                    CalculatePowerCount(transform.position,InPower);
                }
            }
        }

        #endregion

        private new void Awake()
        {
            powerVFXMat = powerSupportVFX.GetComponent<Renderer>().material;
            powerSupportVFX.GetComponent<Renderer>().sharedMaterial = powerVFXMat;
            powerVFXMat.SetColor("_VFXColor",new Color32(139, 167,236,255));
            
            base.Awake();
        }

        void OnDestroy()
        {
            if (BuildPowerIslandManager.Instance != null && BuildPowerIslandManager.Instance.powerSubs.Contains(this))
            {
                InPower = false;
                BuildPowerIslandManager.Instance.powerSubs.Remove(this);
            }
        }

        
        public new void OnChangePlaceEvent(Vector3 oldPos,Vector3 newPos)
        {
            //如果没有，说明刚建造则加入
            if (!BuildPowerIslandManager.Instance.powerSubs.Contains(this))
            {
                BuildPowerIslandManager.Instance.powerSubs.Add(this);
            }
            //原地放
            else if(oldPos == newPos)
            {
                return;
            }
            

            //说明是移动，不是建造
            if (InPower)
            {
                inPower = false;
                Color32 vfxColor = new Color32(139, 167,236,255);
                powerVFXMat.SetColor("_VFXColor",vfxColor);
                //原来的位置powerCount-1
                CalculatePowerCount(oldPos,false);
            }

            foreach (var powerCore in BuildPowerIslandManager.Instance.powerCores)
            {
                if(BuildPowerIslandManager.Instance.CoverEachOther(powerCore,this))
                {
                    InPower = true;
                }
            }
        }

        private void CalculatePowerCount(Vector3 pos,bool isAdd)
        {
            int add;
            if (isAdd) 
                add = 1;
            else 
                add = -1;
            
            foreach (var electAppliance in BuildPowerIslandManager.Instance.electAppliances)
            {
                if(BuildPowerIslandManager.Instance.CoverEachOther(electAppliance,PowerSupportRange,pos))
                    electAppliance.PowerCount += add;
            }
        }
        
        public bool CoverEachOther(ElectAppliance electAppliance, Vector3 powerSubPos)
        {

            return CoverEachOther(electAppliance.PowerSupportRange, PowerSupportRange, 
                electAppliance.transform.position, powerSubPos);
        }
        
        bool CoverEachOther(float r1, float r2, Vector3 pos1, Vector3 pos2)
        {
            float distanceSquared = (pos1 - pos2).sqrMagnitude;
            float radiusSunSquared = (r1 + r2) * (r1 + r2);
            return radiusSunSquared >= distanceSquared;
        }
        

    }
    
}