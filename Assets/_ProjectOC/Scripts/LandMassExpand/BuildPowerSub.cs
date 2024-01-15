using ML.Engine.InventorySystem.CompositeSystem;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using ML.Engine.Manager;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace ML.Engine.BuildingSystem.BuildingPart
{
    //拷贝自BuildingPart，增加OnChangePlaceEvent，放置后更新用电数量

    public class BuildPowerSub : BuildingPart, IPowerBPart, IComposition
    {
        [Header("供电部分")]

        //光圈特效
        
        private int powerSupportRange = 5;
        [ShowInInspector]
        public int PowerSupportRange
        {
            get => powerSupportRange;
            set
            {
                powerSupportRange = value;
                float localScale = value * 0.2f;
                powerSupportVFX.transform.localScale = new Vector3(localScale,1,localScale);
            }
        }
        [SerializeField]
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
  
        private bool inpower;
        [ShowInInspector]
        public bool Inpower
        {
            get=>this.inpower;
            set
            {
                if (value != inpower)
                {
                    inpower = value;
                    //更换颜色
                    Color32 vfxColor = (value ? new Color32(255, 178,126,255): new Color32(139, 167,236,255));
                    powerVFXMat.SetColor("_VFXColor",vfxColor);

                    //在位置不变的情况下，计算附近用电器powercount情况，
                    CalculatePowerCount(transform.position,Inpower);
                }
            }
        }

        #endregion

        private new void Awake()
        {
            BuildPowerIslandManager.Instance.powerSubs.Add(this);
            powerVFXMat = powerSupportVFX.GetComponent<Renderer>().material;
            powerSupportVFX.GetComponent<Renderer>().sharedMaterial = powerVFXMat;
            
            base.Awake();
        }

        void OnDestroy()
        {
            CalculatePowerCount(transform.position,false);
            BuildPowerIslandManager.Instance.powerSubs.Remove(this);
        }

        
        public new void OnChangePlaceEvent(Vector3 oldPos,Vector3 newPos)
        {
            //建造初始为unpower，排除建造
            if (Inpower)
            {
                inpower = false;
                //?如果相等，大概率是建造，而不是移动，不需要移除
                CalculatePowerCount(oldPos,false);
            }
            
            BuildPowerIslandManager.Instance.PowerSupportCalculation();
        }

        private void CalculatePowerCount(Vector3 pos,bool isAdd)
        {
            int add;
            if (isAdd) add = 1;
            else add = -1;
            foreach (var electAppliance in BuildPowerIslandManager.Instance.electAppliances)
            {
                if (CoverEachOther(electAppliance, pos))
                    electAppliance.PowerCount += add;
            }
        }
        
        bool RangeCoverEachOther(float r1, float r2, Vector3 pos1, Vector3 pos2)
        {
            float distanceSquared = (pos1 - pos2).sqrMagnitude;
            float radiusSunSquared = (r1 + r2) * (r1 + r2);
            return radiusSunSquared >= distanceSquared;
        }
        
        public bool CoverEachOther(ElectAppliance electAppliance, Vector3 powerSubPos)
        {

            return RangeCoverEachOther(PowerSupportRange, 0, 
                electAppliance.transform.position, powerSubPos);
        }
    }
    
}