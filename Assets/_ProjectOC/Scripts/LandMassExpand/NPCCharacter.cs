using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
using ML.Engine.InteractSystem;
using ML.Engine.InventorySystem;
using ML.Engine.Manager;
using ML.Engine.Timer;
using ML.PlayerCharacterNS;
using ProjectOC.Dialog;
using ProjectOC.ManagerNS;
using ProjectOC.Player;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ProjectOC.NPC
{
    public class NPCCharacter : MonoBehaviour,IAICharacter, IInteraction, ITickComponent
    {
        #region Tick Register

        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }
        private void Awake()
        {
            GameManager.Instance.TickManager.RegisterTick(0, this);

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

        public string InteractType { get; set; } = "NPCCharacter";
        public Vector3 PosOffset { get; set; } = Vector3.zero;
        
        private string currentDialogueID = "";
        
        public void Interact(ML.Engine.InteractSystem.InteractComponent component)
        {
            LocalGameManager.Instance.DialogManager.StartDialogMode(currentDialogueID);
        }
        
        #endregion

        #region Dialog
        
        private void EndDialogMode()
        {
            
        }
        
        #endregion
        #region Character

        public int prefabIndex { get; }
        public ICharacterState State { get; set; }
        public IController Controller { get; set; }
        public void OnSpawn(IController controller)
        {
        }

        public void OnDespose(IController controller)
        {
        }

        #endregion

    }
}
