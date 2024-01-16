using ML.Engine.InventorySystem.CompositeSystem;
using ML.Engine.BuildingSystem.BuildingPart;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.LandMassExpand
{
    //������BuildingPart������OnChangePlaceEvent�����ú�����õ�����

    public class BuildPowerSub : BuildingPart, IPowerBPart, IComposition
    {
        [Header("���粿��")]

        //��Ȧ��Ч
        
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
  
        private bool inpower = false;
        [ShowInInspector]
        public bool Inpower
        {
            get=>this.inpower;
            set
            {
                if (value != inpower)
                {
                    inpower = value;
                    //������ɫ
                    Color32 vfxColor = (value ? new Color32(255, 178,126,255): new Color32(139, 167,236,255));
                    powerVFXMat.SetColor("_VFXColor",vfxColor);

                    //��λ�ò��������£����㸽���õ���powercount�����
                    CalculatePowerCount(transform.position,Inpower);
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
                Inpower = false;
                BuildPowerIslandManager.Instance.powerSubs.Remove(this);
            }
        }

        
        public new void OnChangePlaceEvent(Vector3 oldPos,Vector3 newPos)
        {
            //���û�У�˵���ս��������
            if (!BuildPowerIslandManager.Instance.powerSubs.Contains(this))
            {
                BuildPowerIslandManager.Instance.powerSubs.Add(this);
            }
            
#if UNITY_EDITOR
            float startT = Time.realtimeSinceStartup;
#endif
            //˵�����ƶ������ǽ���
            if (Inpower)
            {
                inpower = false;
                Color32 vfxColor = new Color32(139, 167,236,255);
                powerVFXMat.SetColor("_VFXColor",vfxColor);
                CalculatePowerCount(oldPos,false);
            }
#if UNITY_EDITOR
            Debug.Log($"CalculatePowerCount cost time: {Time.realtimeSinceStartup - startT}");          
#endif
            BuildPowerIslandManager.Instance.PowerSupportCalculation();
            
#if UNITY_EDITOR
            //������0.02
            Debug.Log($"CalculatePowerCount + PowerSub cost time: {Time.realtimeSinceStartup - startT}");          
#endif        
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
                if (CoverEachOther(electAppliance, pos))
                    electAppliance.PowerCount += add;
            }
        }
        
        public bool CoverEachOther(ElectAppliance electAppliance, Vector3 powerSubPos)
        {

            return CoverEachOther(PowerSupportRange, 0, 
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