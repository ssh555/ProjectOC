using System;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Movement;
using ML.Engine.InventorySystem.CompositeSystem;
using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.Manager;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.LandMassExpand
{
    //拷贝自BuildingPart，增加OnChangePlaceEvent，放置后更新用电数量

    public class BuildPowerSub : BuildingPart, INeedPowerBpart, ISupportPowerBPart,IComposition
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
        //存储周围NeedPowerBpart
        public List<INeedPowerBpart> needPowerBparts { get; private set; }
        private BuildPowerIslandManager bpIslandManager;
        
        #region IPowerBPart

        private int powerCount;
        public int PowerCount
        {
            get=>powerCount;
            set
            {
                powerCount = value;
                if (powerCount > 0)
                    InPower = true;
                else
                    InPower = false;
            }
        }

        private bool inPower = false;
        [ShowInInspector]
        public bool InPower
        {
            get => this.inPower;
            protected set
            {
                if (value != inPower)
                {
                    ChangePowerCount(value);
                    inPower = value;
                    //更换颜色
                    Color32 vfxColor = (value ? new Color32(255, 178,126,255): new Color32(139, 167,236,255));
                    powerVFXMat.SetColor("_VFXColor",vfxColor);
                }
            }
        }

        #endregion

        protected override void Awake()
        {
            powerVFXMat = powerSupportVFX.GetComponent<Renderer>().material;
            powerSupportVFX.GetComponent<Renderer>().sharedMaterial = powerVFXMat;
            powerVFXMat.SetColor("_VFXColor",new Color32(139, 167,236,255));
            needPowerBparts = new List<INeedPowerBpart>();
            
            base.Awake();
        }

        protected virtual void Start()
        {
            bpIslandManager = GameManager.Instance.GetLocalManager<BuildPowerIslandManager>();
        }

        void OnDestroy()
        {
            if (bpIslandManager != null && bpIslandManager.powerSubs.Contains(this))
            {
                PowerCount = 0;
                RemoveFromAllPowerCores();
  
                bpIslandManager.powerSubs.Remove(this);
            }
        }

        
        public override void OnChangePlaceEvent(Vector3 oldPos,Vector3 newPos)
        {
            //如果没有，说明刚建造则加入
            if (isFirstBuild)
            {
                bpIslandManager.powerSubs.Add(this);
            }
            
            //重置 powerCore.needPowers、PowerCount、needPowers
            RemoveFromAllPowerCores();
            PowerCount = 0;            
            needPowerBparts.Clear();
            
            //重新计算powerCore
            foreach (var powerCore in bpIslandManager.powerCores)
            {
                if(bpIslandManager.CoverEachOther(powerCore,this))
                {
                    powerCore.needPowerBparts.Add(this);
                    PowerCount++;
                }
            }
            CalculatePowerCount();
            base.OnChangePlaceEvent(oldPos,newPos);
        }

        private void ChangePowerCount(bool isAdd)
        {
            int add;
            if (isAdd) 
                add = 1;
            else 
                add = -1;

            foreach (var needPowerBpart in needPowerBparts)
            {
                needPowerBpart.PowerCount += add;
            }
        }
        
        //重新计算附近
        private void CalculatePowerCount()
        {
            foreach (var electAppliance in bpIslandManager.electAppliances)
            {
                if (CoverEachOther(electAppliance,transform.position))
                {
                    needPowerBparts.Add(this);
                    if(InPower)
                        electAppliance.PowerCount++;
                }
            }
        }
        
        public bool CoverEachOther(INeedPowerBpart electAppliance, Vector3 powerSubPos)
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

        public void RemoveFromAllPowerCores()
        {
            foreach (var powerCore in bpIslandManager.powerCores)
            {
                powerCore.RemoveNeedPowerBpart(this);
            }
        }
    }
    
}