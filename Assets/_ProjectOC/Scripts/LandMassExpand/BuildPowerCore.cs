using Sirenix.OdinInspector;
using UnityEngine;
using ML.Engine.InventorySystem.CompositeSystem;
using ML.Engine.BuildingSystem.BuildingPart;


namespace ProjectOC.LandMassExpand
{
    public class BuildPowerCore : BuildingPart,ISupportPowerBPart
    {
        [Header("���粿��"),SerializeField,LabelText("���緶Χ��Ч")] 
        private Transform powerSupportVFX;
        
        private float powerSupportRange = 20;

        [LabelText("���緶Χ"),ShowInInspector]
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

        public override void OnChangePlaceEvent(Vector3 oldPos, Vector3 newPos)
        {
            //���Listû�У�˵���ս��������
            if (!BuildPowerIslandManager.Instance.powerCores.Contains(this))
            {
                BuildPowerIslandManager.Instance.powerCores.Add(this);
            }
            
            foreach (var powerSub in BuildPowerIslandManager.Instance.powerSubs)
            {
                if (BuildPowerIslandManager.Instance.CoverEachOther(powerSub, this))
                {
                    powerSub.InPower = true;
                }

            }
        }
    }
}
