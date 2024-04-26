using System;
using System.Collections;
using System.Collections.Generic;
using ML.Engine.Manager;
using ProjectOC.ManagerNS;
using ProjectOC.Player;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace ProjectOC.PinchFace
{
    [System.Serializable]
    public class PinchFaceManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        //引用
        public PinchFaceHelper pinchFaceHelper;
        public CharacterModelPinch ModelPinch;
        
        [ShowInInspector]
        public Dictionary<PinchPartType3, PinchPartType2> pinchPartType3Dic = new Dictionary<PinchPartType3, PinchPartType2>();
        public Dictionary<PinchPartType2, PinchPartType> pinchPartType2Dic = new Dictionary<PinchPartType2, PinchPartType>();
        //存储每一个的Type1下的每一个Type2
        [SerializeField]
        public List<List<PinchPartType2>> pinchPartType1Inclusion;  //

        // public List<PinchType1Struct> PinchType1Structs;
        private const string PinchPartTypePath = "PinchFace_TypePackage";
        private const string PinchPartPath = "PinchFacePart";
        private AsyncOperationHandle PinchPartHandle;
        private AsyncOperationHandle PinchPartTypeHandle;

        public void OnRegister()
        {
            DataStructInit();
            RegisterPinchPartType();
            pinchFaceHelper = new PinchFaceHelper(this);
            
            // GeneratePinchRaceUI();
            // GenerateCustomRaceUI();
            //GeneratePinchFaceUI(); 
        }

        public void UnRegister()
        {
            
        }

        void DataStructInit()
        {
            //1-6,不需要存0
            pinchPartType1Inclusion = new List<List<PinchPartType2>>(Enum.GetValues(typeof(PinchPartType1)).Length-1);
            for (int i = 0; i < pinchPartType1Inclusion.Capacity; i++)
            {
                pinchPartType1Inclusion.Add(new List<PinchPartType2>());
            }
        }
        /// <summary>
        /// PinchPart相关字典加载
        /// </summary>
        public void RegisterPinchPartType()
        {
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetsAsync<PinchPartType>(PinchPartTypePath, (ppt) =>
            {
                lock (pinchPartType3Dic)
                {
                    lock (pinchPartType2Dic)
                    {
                        pinchPartType2Dic.Add(ppt.pinchPartType2,ppt);
                        pinchPartType1Inclusion[(int)ppt.pinchPartType1 -1].Add(ppt.pinchPartType2);
                        foreach (var _ppt3 in ppt.pinchPartType3s)
                        {
                            pinchPartType3Dic.Add(_ppt3,ppt.pinchPartType2);
                        }
                    } 
                }
                
            }).Completed+= (handle) =>
            {
                PinchPartTypeHandle = handle;
            };
        }
        

        #region Temp

        private const string PinchRaceUIPath = "Prefabs_PinchPart/UIPanel/Panel/Prefab_FacePinch_RacePanel.prefab";
        private const string CustomRacePath = "Prefabs_PinchPart/UIPanel/Panel/Prefab_FacePinch_RacePartPanel.prefab";
        private const string PinchFacePath = "Prefabs_PinchPart/UIPanel/Panel/Prefab_FacePinch_FacePanel.prefab";
        
        [SerializeField]
        public List<RacePinchData> RacePinchDatas;
        
        public void GeneratePinchRaceUI()
        {
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(PinchRaceUIPath)
                .Completed += (handle) =>
            {
                var panel = handle.Result.GetComponent<UIPinchRacePanel>();
                panel.transform.SetParent(ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false);
                ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);
            };
        }
        public void GenerateCustomRaceUI()
        {
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(CustomRacePath)
                .Completed += (handle) =>
            {
                var panel = handle.Result.GetComponent<UICustomRacePanel>();
                panel.transform.SetParent(ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false);
                ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);
                
            };
        }

        public void GeneratePinchFaceUI()
        {
            GeneratePinchFaceUI(RacePinchDatas[0]); 
        }
        public void GeneratePinchFaceUI(RacePinchData _raceData)
        {
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(PinchFacePath)
                .Completed += (handle) =>
            {
                var panel = handle.Result.GetComponent<UIPinchFacePanel>();
                panel.transform.SetParent(ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false);
                ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);

                panel.InitRaceData(_raceData);
            };
        }
        
        #endregion
    }
}
