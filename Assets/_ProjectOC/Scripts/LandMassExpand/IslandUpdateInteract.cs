using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
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
        
        public void Interact(ML.Engine.InteractSystem.InteractComponent component)
        {
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(islandUpdatePanelPath)
                .Completed +=(handle) =>
            {
                UIIslandUpdatePanel uiPanel = handle.Result.GetComponent<UIIslandUpdatePanel>();
                ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(uiPanel);


                foreach (var _condition in BuildPowerIslandManager.CurrentLandLevelData.Conditions)
                {
                    GameManager.Instance.EventManager.ExecuteCondition(_condition);
                }
            };
        }
        
        
        
        #endregion
    }
}
