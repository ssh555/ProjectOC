using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
using ML.Engine.InteractSystem;
using ML.Engine.InventorySystem;
using ML.Engine.Manager;
using ML.Engine.Timer;
using ProjectOC.Player;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ML.Engine.InventorySystem
{
    public class CollectiveActor : MonoBehaviour, IInteraction, ML.Engine.Timer.ITickComponent
    {
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }


        private string ID;
        private string Name;
        private string CollectText;
        public List<DropItem> DropItems;

        [System.Serializable]
        public struct DropItem
        {
            public string dropItem;
            public int num;
        }

        public int collectCount;
        private int currentCollectCount;
        private bool interactStorage = false;
        private bool inCollective = false;

        private PlayerCharacter character;
        private PlayerCharacter Character
        {
            get
            {
                if (character == null)
                {
                    character = (GameManager.Instance.CharacterManager.GetLocalController() as OCPlayerController)
                        .currentCharacter;
                }

                return character;
            }
        }
        private string collectUIPath = "InteractSystem/Prefab_InteractSystem_Outline.prefab";
        private GameObject collectUIPrefab;
        private InteractComponent interactComponent;
        private GameObject OutlineGameObejct;

        private void Awake()
        {
            GameManager.Instance.TickManager.RegisterTick(0, this);
            Input.InputManager.Instance.Common.Common.ConfirmHold.performed += OnLongPressStarted;
            this.enabled = false;
        }

        private void OnDestroy()
        {
            Input.InputManager.Instance.Common.Common.ConfirmHold.performed -= OnLongPressStarted;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TickManager.UnregisterTick(this);   
            }
        }
        void OnLongPressStarted(InputAction.CallbackContext context)
        {
            if (inCollective)
            {
                interactStorage = true;   
            }
        }
        
        
        public void Tick(float deltatime)
        {
            if (OutlineGameObejct != null)
            {
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position + PosOffset);
                screenPosition.x = Mathf.Clamp(screenPosition.x, 0, Screen.width);
                screenPosition.y = Mathf.Clamp(screenPosition.y, 0, Screen.height);
                OutlineGameObejct.transform.position = screenPosition;
            }

            if (inCollective)
            {
                AnimatorStateInfo stateInfo =
                    Character.playerModelStateController.modelAnimator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.normalizedTime >= 1f)
                {
                    AfterAnimLoop();
                }
            }
        }

        void BeginCollect(InteractComponent component)
        {
            //开始采集
            inCollective = true;
            Character.playerModelStateController.modelAnimator.Play("Pick");

            interactComponent = component;
            GameManager.Instance.ABResourceManager
                .InstantiateAsync(collectUIPath).Completed += (handle) =>
            {
                OutlineGameObejct = handle.Result;
                OutlineGameObejct.transform.SetParent(GameManager.Instance.UIManager.GetCanvas.transform);

                OutlineGameObejct.transform.GetChild(0).GetComponent<Image>().fillAmount =
                    (float)currentCollectCount / collectCount;
            };
            ProjectOC.Input.InputManager.PlayerInput.Player.Disable();
        }

        void StopCollect()
        {
            //修改UI
            inCollective = false;
            Character.playerModelStateController.modelAnimator.Play("Idle_0");

            interactComponent = null;

            Destroy(OutlineGameObejct);
            ProjectOC.Input.InputManager.PlayerInput.Player.Enable();
        }

        void AfterAnimLoop()
        {
            currentCollectCount++;
            if (currentCollectCount >= collectCount)
            {
                PickUp();
                StopCollect();
                return;
            }


            if (!interactStorage)
            {
                StopCollect();
            }
            else
            {
                Character.playerModelStateController.modelAnimator.Play("Pick", 0, 0f);
                OutlineGameObejct.transform.GetChild(0).GetComponent<Image>().fillAmount =
                    (float)currentCollectCount / collectCount;
            }


            interactStorage = false;

        }



        private void PickUp()
        {
            IInventory inventory = Character.Inventory;
            foreach (var dropItem in DropItems)
            {
                if (ItemManager.Instance.IsValidItemID(dropItem.dropItem))
                {
                    var item = ItemManager.Instance.SpawnItem(dropItem.dropItem);
                    item.Amount = dropItem.num;
                    inventory.AddItem(item);
                    Destroy(this.gameObject);
                }
                else
                {
                    throw new System.Exception(this.gameObject.name + " Item == null !!!");
                }
            }
        }

        public void Interact(ML.Engine.InteractSystem.InteractComponent component)
        {
            if (!inCollective)
            {
                BeginCollect(component);
            }
            else
            {
                interactStorage = true;
            }
        }

        #region Interact

        public string InteractType { get; set; } = "CollectiveActor";
        public Vector3 PosOffset { get; set; } = Vector3.zero;

        #endregion
    }
}
