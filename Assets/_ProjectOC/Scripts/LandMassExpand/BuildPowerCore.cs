using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using ML.Engine.InventorySystem.CompositeSystem;
using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.Manager;
using UnityEngine.Serialization;


namespace ProjectOC.LandMassExpand
{
    public class BuildPowerCore : BuildingPart,ISupportPowerBPart
    {
        [Header("供电部分"),SerializeField,LabelText("供电范围特效")] 
        private Transform powerSupportVFX;
        
        [SerializeField,HideInInspector]
        private float powerSupportRange = 20;
        [LabelText("供电范围"),ShowInInspector]
        public float PowerSupportRange
        {
            get => powerSupportRange;
            set
            {
                powerSupportRange = value;
                float _localScale = value * 0.2f;
                powerSupportVFX.transform.localScale = new Vector3(_localScale,1,_localScale);
            }
        }

        public bool InPower => true;

        public List<INeedPowerBpart> needPowerBparts { get; private set; }
        private BuildPowerIslandManager bPIslandManager;
        protected override void Awake()
        {
            needPowerBparts = new List<INeedPowerBpart>();
            base.Awake();
        }

        protected void Start()
        {
            bPIslandManager = GameManager.Instance.GetLocalManager<BuildPowerIslandManager>();
        }

        void OnDestroy()
        {
            if (bPIslandManager != null && bPIslandManager.powerCores.Contains(this))
            {
                bPIslandManager.powerCores.Remove(this);
                
                foreach (var needPowerBpart in needPowerBparts)
                {
                    needPowerBpart.PowerCount--;
                }
            }
            
        }

        public override void OnChangePlaceEvent(Vector3 oldPos, Vector3 newPos)
        {
            //如果List没有，说明刚建造则加入
            if (!bPIslandManager.powerCores.Contains(this))
            {
                bPIslandManager.powerCores.Add(this);
            }
            else
            {
                //减少旧的powerCount，添加新的
                foreach (var needPowerBpart in needPowerBparts)
                {
                    needPowerBpart.PowerCount--;
                }    
            }
            
            //移出
            needPowerBparts.Clear();
            //添加新的
            foreach (var electAppliance in bPIslandManager.electAppliances)
            {
                if (bPIslandManager.CoverEachOther(electAppliance, this))
                {
                    needPowerBparts.Add(electAppliance);
                    electAppliance.PowerCount++;
                }
            }
            foreach (var powerSub in bPIslandManager.powerSubs)
            {
                if (bPIslandManager.CoverEachOther(powerSub, this))
                {
                    needPowerBparts.Add(powerSub);
                    powerSub.PowerCount++;
                }
            }
        }
    }
}
