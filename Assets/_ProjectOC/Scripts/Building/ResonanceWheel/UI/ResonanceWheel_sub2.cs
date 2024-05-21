using ML.Engine.Input;
using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.UI;
using ProjectOC.ManagerNS;
using ProjectOC.WorkerNS;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using static ProjectOC.ResonanceWheelSystem.UI.ResonanceWheel_sub2;
using static ProjectOC.ResonanceWheelSystem.UI.ResonanceWheelUI;

namespace ProjectOC.ResonanceWheelSystem.UI
{
    public class ResonanceWheel_sub2 : ML.Engine.UI.UIBasePanel<ResonanceWheel_sub2Struct>
    {
        #region Unity
        public bool IsInit = false;

        protected override void Awake()
        {
            base.Awake();

            //Ring
            var ringcontent = this.transform.Find("Ring").Find("Viewport").Find("Content");

            ring = ringcontent.Find("Ring").Find("ring");

            Grids = new GridBeastType[ring.childCount];
            IsInit = true;
            Refresh();
        }

        #endregion

        #region Override
        public override void OnEnter()
        {
            base.OnEnter();
            parentUI.MainToSub2();
        }

        public override void OnExit()
        {
            base.OnExit();
            ClearTemp();
            parentUI.Sub2ToMain();
        }
        #endregion

        private struct GridBeastType
        {
            public Transform transform;
            public WorkerCategory beastType;
            public Image image;

            public GridBeastType(Transform transform, WorkerCategory beastType,Image image)
            {
                this.transform = transform;
                this.beastType = beastType;
                this.image = image;
            }

        }
        #region Internal
        protected override void UnregisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub2.Disable();

            //�л�����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub2.NextGrid.performed -= NextGrid_performed;

            //ȷ��
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub2.Confirm.performed -= Confirm_performed;

            // ����
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
        }

        protected override void RegisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub2.Enable();
            //�л�����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub2.NextGrid.performed += NextGrid_performed;

            //ȷ��
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI_sub2.Confirm.performed += Confirm_performed;

            // ����
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            UIMgr.PopPanel();
        }

        private void NextGrid_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {

            Vector2 vector2 = obj.ReadValue<Vector2>();

            float angle = Mathf.Atan2(vector2.x, vector2.y);

            angle = angle * 180 / Mathf.PI;
            if (angle < 0)
            {
                angle = angle + 360;
            }

            if (angle < 25.7 || angle > 334.3) CurrentGridIndex = 0;
            else if (angle > 25.7 && angle < 77.1) CurrentGridIndex = 6;
            else if (angle > 77.1 && angle < 128.5) CurrentGridIndex = 5;
            else if (angle > 128.5 && angle < 179.9) CurrentGridIndex = 4;
            else if (angle > 179.9 && angle < 231.3) CurrentGridIndex = 3;
            else if (angle > 231.3 && angle < 282.7) CurrentGridIndex = 2;
            else if (angle > 282.7 && angle < 334.3) CurrentGridIndex = 1;

            this.Refresh();
        }

        private void Confirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            
            
            parentUI.Grids[parentUI.CurrentGridIndex].beastType = Grids[CurrentGridIndex].beastType;
            parentUI.currentBeastType = Grids[CurrentGridIndex].beastType;
            UIMgr.PopPanel();
            
        }
        #endregion

        #region UI
        #region Temp
        private List<Sprite> tempSprite = new List<Sprite>();
        private Dictionary<ML.Engine.InventorySystem.ItemType, GameObject> tempItemType = new Dictionary<ML.Engine.InventorySystem.ItemType, GameObject>();
        private List<GameObject> tempUIItems = new List<GameObject>();

        private void ClearTemp()
        {
            foreach (var s in tempSprite)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
            }
            foreach (var s in tempItemType.Values)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
            }
            foreach (var s in tempUIItems)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
            }

        }

        #endregion

        #region UI��������
        public ResonanceWheelUI parentUI = null;

        private Transform ring;
        //ring
        [ShowInInspector]
        private GridBeastType[] Grids;
        private bool isInitGridsSprite = false;

        private int CurrentGridIndex = 0;//0��5

        #endregion

        public override void Refresh()
        {
            if (ABJAProcessorJson == null || !ABJAProcessorJson.IsLoaded || !IsInit)
            {
                return;
            }

            //ring

            if(isInitGridsSprite == false)
            {
                for (int i = 0; i < ring.childCount; i++)
                {
                    Grids[i].transform = ring.GetChild(i);
                    Grids[i].beastType = (WorkerCategory)Enum.Parse(typeof(WorkerCategory), ring.GetChild(i).name);
                    Grids[i].image = Grids[i].transform.Find("Image").GetComponent<Image>();
                    Grids[i].image.sprite = parentUI.beastTypeDic[Grids[i].beastType];
                }
                isInitGridsSprite = true;
            }


            for (int i = 0; i < Grids.Length; i++)
            {
                if (CurrentGridIndex == i)
                {
                    Grids[i].transform.Find("Selected").gameObject.SetActive(true);
                }
                else
                {
                    Grids[i].transform.Find("Selected").gameObject.SetActive(false);
                }
            }

        }
        #endregion


        #region Resource
        #region TextContent
        [System.Serializable]
        public struct ResonanceWheel_sub2Struct
        {
            //BotKeyTips
            public KeyTip Confirm;
            public KeyTip Back;
        }

        protected override void InitTextContentPathData()
        {
            this.abpath = "OCTextContent/ResonanceWheel";
            this.abname = "ResonanceWheel_sub2";
            this.description = "ResonanceWheel_sub2���ݼ������";
        }

        #endregion
        #endregion
    }

}
