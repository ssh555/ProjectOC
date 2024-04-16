using System;
using System.Collections;
using System.Collections.Generic;
using ProjectOC.ManagerNS;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;

namespace ProjectOC.PinchFace
{
    [System.Serializable]
    public class PinchFaceManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        [ShowInInspector]
        public Dictionary<PinchPartType3, PinchPartType> pinchPartType3Dic;
        public Dictionary<PinchPartType2, PinchPartType1> pinchPartType2Dic;
        [ShowInInspector]
        public List<List<PinchPartType3>> pinchPartType2Inclusion;  //
        private const string PinchPartTypePath = "PinchFaceType";
        private const string PinchPartPath = "PinchFacePart";
        private AsyncOperationHandle PinchPartHandle;
        private AsyncOperationHandle PinchPartTypeHandle;

        public void OnRegister()
        {
            DataStructInit();
            RegisterPinchPartType();
            // GeneratePinchRaceUI();
            GenerateCustomRaceUI();
        }

        public void UnRegister()
        {
            
        }

        void DataStructInit()
        {
            pinchPartType2Inclusion = new List<List<PinchPartType3>>(Enum.GetValues(typeof(PinchPartType2)).Length);
            for (int i = 0; i < pinchPartType2Inclusion.Capacity; i++)
            {
                pinchPartType2Inclusion.Add(new List<PinchPartType3>());
            }
        }
        /// <summary>
        /// PinchPart相关字典加载
        /// </summary>
        public void RegisterPinchPartType()
        {
            pinchPartType3Dic = new Dictionary<PinchPartType3, PinchPartType>();
            pinchPartType2Dic = new Dictionary<PinchPartType2, PinchPartType1>();
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetsAsync<PinchPartType>(PinchPartTypePath, (ppt) =>
            {
                lock (pinchPartType3Dic)
                {
                    pinchPartType3Dic.Add(ppt.pinchPartType3,ppt);
                }

                lock (pinchPartType2Dic)
                {
                    pinchPartType2Dic[ppt.pinchPartType2] = ppt.pinchPartType1;
                }
                pinchPartType2Inclusion[(int)ppt.pinchPartType2 -1].Add(ppt.pinchPartType3);
            }).Completed+= (handle) =>
            {
                PinchPartTypeHandle = handle;
            };
        }
        
        //考虑根据PinchPartType3 去找对应的文件夹
        // public void RegisterPinchPartPrefab()
        // {
        //     ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetsAsync<GameObject>(PinchPartPath, (obj) =>
        //     {
        //         lock (pinchPartType)
        //         {
        //             
        //         }
        //     }).Completed+= (handle) =>
        //     {
        //         PinchPartHandle = handle;
        //     };
        // }
        /// <summary>
        /// 值 Remap
        /// </summary>
        public static float RemapValue(float value, float fromMin, float fromMax, float toMin = 0f, float toMax = 1f)
        {
            // 将 value 从 fromMin 到 fromMax 范围映射到 toMin 到 toMax 范围
            return toMin + (value - fromMin) / (fromMax - fromMin) * (toMax - toMin);
        }

        #region 捏脸数据结构体

        
        #endregion

        #region Temp

        private const string PinchRaceUIPath = "OC/UIPanel/FacePinch_RacePanel.prefab";
        private const string CustomRacePath = "OC/UIPanel/FacePinch_RacePartPanel.prefab";
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
                Debug.LogWarning(panel.name);
                ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);
                
            };
        }

        #endregion
    }
}
