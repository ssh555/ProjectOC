using Sirenix.OdinInspector;
using UnityEngine;
using ML.Engine.InventorySystem.CompositeSystem;
using ML.Engine.BuildingSystem.BuildingPart;


namespace ProjectOC.LandMassExpand
{
    public class BuildPowerCore : BuildingPart,ISupportPowerBPart
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
        public bool InPower { get; set; }

        private new void Awake()
        {
            base.Awake();
        }
        void OnDestroy()
        {
            if (BuildPowerIslandManager.Instance != null && BuildPowerIslandManager.Instance.powerCores.Contains(this))
            {
                BuildPowerIslandManager.Instance.powerCores.Remove(this);
            }
            
        }

        public new void OnChangePlaceEvent(Vector3 oldPos, Vector3 newPos)
        {
            //如果List没有，说明刚建造则加入
            if (!BuildPowerIslandManager.Instance.powerCores.Contains(this))
            {
                BuildPowerIslandManager.Instance.powerCores.Add(this);
            }
            BuildPowerIslandManager.Instance.PowerSupportCalculation();
        }
    }
}
