using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
using ML.Engine.BuildingSystem;
using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InteractSystem;
using ML.Engine.InventorySystem;
using ML.Engine.Manager;
using ML.Engine.Timer;
using ProjectOC.ManagerNS;
using ProjectOC.Player;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ProjectOC.LandMassExpand
{
    public class IslandUpdateInteract : MonoBehaviour, IInteraction, ML.Engine.Timer.ITickComponent
    {
        #region Tick Register

        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }
        private void Awake()
        {
            GameManager.Instance.TickManager.RegisterTick(0, this);
        }

        private void Start()
        {
            BuildPowerIslandManager = LocalGameManager.Instance.BuildPowerIslandManager;
            this.enabled = false;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TickManager.UnregisterTick(this);   
            }
        }
        
        
        public void Tick(float deltatime)
        {
    
        }
        #endregion
        

        #region Interact

        public string InteractType { get; set; } = "IslandUpdate";
        public Vector3 PosOffset { get; set; } = Vector3.zero;
        
        private string islandUpdatePanelPath = "Prefab_IslandExpand/UI/Prefab_IslandUpdatePanel.prefab";
        private BuildPowerIslandManager BuildPowerIslandManager; 
        public bool CouldUpdate { get; private set; }
        public void Interact(ML.Engine.InteractSystem.InteractComponent component)
        {
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(islandUpdatePanelPath)
                .Completed +=(handle) =>
            {
                UIIslandUpdatePanel uiPanel = handle.Result.GetComponent<UIIslandUpdatePanel>();
                uiPanel.transform.SetParent(ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false);
                ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(uiPanel);

                IslandInfoUpdate(uiPanel);
            };
        }

        public void IslandInfoUpdate(UIIslandUpdatePanel _islandUpdatePanel)
        {
            //text更新
            string eventText = "";
            foreach (var _text in BuildPowerIslandManager.CurrentLandLevelData.EventTexts)
            {
                eventText += $"{_text}\n";
            }
            _islandUpdatePanel.SetLevelInfo(BuildPowerIslandManager.currentLandLevel,BuildPowerIslandManager.CurrentLandLevelData.LevelText,eventText);
            
            //Condition判定
            bool _isFinished = true;
            string[] _Conditions = BuildPowerIslandManager.CurrentLandLevelData.Conditions;
            for (int i = 0; i < _Conditions.Length; i++)
            {
                bool finishOneTarget = GameManager.Instance.EventManager.ExecuteCondition(_Conditions[i]);
                //更新text
                //Condition_CheckBuild_LifeDiversion_1
                string buildingTypeStr = _Conditions[i].Split("_")[2];
                BuildingCategory2 buildingType =  (BuildingCategory2)Enum.Parse(typeof(Color), buildingTypeStr);
                int currentCount = BuildingManager.Instance.GetBuildingCount(buildingType);
                //string _conditionText = BuildPowerIslandManager.CurrentLandLevelData.ConditionTexts[i].GetText().Replace("&P1",currentCount.ToString());
                string _conditionText = "";//EventManager.get
                _islandUpdatePanel.SetTargetInfo(_conditionText,finishOneTarget);
                
                _isFinished &= finishOneTarget;
            }
            CouldUpdate = _isFinished;
        }
        
        
        
        
        #endregion
    }
}
