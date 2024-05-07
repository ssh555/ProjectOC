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
using Sirenix.OdinInspector;
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
        private void Start()
        {
            GameManager.Instance.TickManager.RegisterTick(0, this);
            DialogManager = LocalGameManager.Instance.DialogManager;
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
        
        private string dialogID = "Dialog_BeginnerTutorial_0";
        
        public void Interact(ML.Engine.InteractSystem.InteractComponent component)
        {
            DialogManager.StartDialogMode(dialogID);
        }
        
        #endregion

        #region Dialog

        private DialogManager DialogManager;
        [SerializeField,FoldoutGroup("Transform引用")]
        private SkinnedMeshRenderer smr;
        [SerializeField,FoldoutGroup("Transform引用")]
        private Animator modelAnimator;
        public void EndDialogMode()
        {
            //动作 面部表情复原
            modelAnimator.Play("Calm");
            
            smr.SetBlendShapeWeight(0,0f);
            smr.SetBlendShapeWeight(1,0f);
        }

        public void PlayAction(string _animID)
        {
            _animID = _animID.Replace("Action_", "");
            if (_animID != "")
            {
                modelAnimator.Play(_animID);   
            }
        }
        public void PlayMood(string _moodID)
        {
            if (_moodID == "Mood_Smile")
            {
                smr.SetBlendShapeWeight(0,100f);
            }
            else if (_moodID == "Mood_Calm")
            {
                smr.SetBlendShapeWeight(0,100f);
            }
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
