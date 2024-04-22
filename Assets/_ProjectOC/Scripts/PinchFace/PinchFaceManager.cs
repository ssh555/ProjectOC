using System;
using System.Collections;
using System.Collections.Generic;
using ML.Engine.Manager;
using ProjectOC.ManagerNS;
using ProjectOC.Player;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;
using ProjectOC.Player;

namespace ProjectOC.PinchFace
{
    [System.Serializable]
    public class PinchFaceManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        //引用
        public PinchFaceHelper pinchFaceHelper;
        public CharacterModelPinch ModelPinch;
        [ShowInInspector]
        //public Dictionary<PinchPartType3, PinchPartType> pinchPartType3Dic;
        public Dictionary<PinchPartType2, PinchPartType> pinchPartType2Dic;
        //存储每一个的Type1下的每一个Type2
        [SerializeField]
        public List<List<PinchPartType2>> pinchPartType1Inclusion;  //

        // public List<PinchType1Struct> PinchType1Structs;
        private const string PinchPartTypePath = "PinchFaceType";
        private const string PinchPartPath = "PinchFacePart";
        private AsyncOperationHandle PinchPartHandle;
        private AsyncOperationHandle PinchPartTypeHandle;

        public void OnRegister()
        {
            ModelPinch = (GameManager.Instance.CharacterManager.GetLocalController() as OCPlayerController)
                .currentCharacter.GetComponentInChildren<CharacterModelPinch>();
            DataStructInit();
            RegisterPinchPartType();
            pinchFaceHelper = new PinchFaceHelper(this);
            
            
            // GeneratePinchRaceUI();
            // GenerateCustomRaceUI();
            //GeneratePinchFaceUI(RacePinchDatas[0]);
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
            // pinchPartType3Dic = new Dictionary<PinchPartType3, PinchPartType>();
            pinchPartType2Dic = new Dictionary<PinchPartType2, PinchPartType>();
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetsAsync<PinchPartType>(PinchPartTypePath, (ppt) =>
            {
                lock (pinchPartType2Dic)
                {
                    //有效的Capacity
                    if (ppt.pinchPartType3s.Capacity != 0)
                    {
                        pinchPartType2Dic.Add(ppt.pinchPartType2,ppt);
                        pinchPartType1Inclusion[(int)ppt.pinchPartType1 -1].Add(ppt.pinchPartType2);
                    }
                }
            }).Completed+= (handle) =>
            {
                PinchPartTypeHandle = handle;
            };
        }
        

        #region Temp

        private const string PinchRaceUIPath = "OC/UIPanel/FacePinch_RacePanel.prefab";
        private const string CustomRacePath = "OC/UIPanel/FacePinch_RacePartPanel.prefab";
        private const string PinchFacePath = "OC/UIPanel/FacePinch_FacePanel.prefab";
        
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
