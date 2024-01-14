using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InventorySystem;
using ML.Engine.TextContent;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace ProjectOC.ResonanceWheelSystem.UI
{
    public class ResonanceWheel_sub1 : ML.Engine.UI.UIBasePanel
    {

        public IInventory inventory;

        #region Input
        /// <summary>
        /// 用于Drop和Destroy按键响应Cancel
        /// 长按响应了Destroy就置为true
        /// Cancel就不响应Drop 并 重置
        /// </summary>

        #endregion

        #region Unity
        public bool IsInit = false;
        private void Start()
        {
            InitUITextContents();



            


            IsInit = true;
            Refresh();
        }

        #endregion

        #region Override
        public override void OnEnter()
        {
            base.OnEnter();
            this.Enter();
        }

        public override void OnExit()
        {
            base.OnExit();
            this.Exit();
            ClearTemp();
        }

        public override void OnPause()
        {
            base.OnPause();
            this.Exit();
        }

        public override void OnRecovery()
        {
            base.OnRecovery();
            this.Enter();
        }

        #endregion

        #region Internal
 

   
        private int CurrentFuctionTypeIndex = 0;//0为HBR 1为SSB
        private int CurrentGridIndex = 0;//0到4


        private void Enter()
        {
            this.RegisterInput();
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.Enable();
            ML.Engine.Manager.GameManager.Instance.SetAllGameTimeRate(0);
            this.Refresh();
        }

        private void Exit()
        {
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.Disable();
            ML.Engine.Manager.GameManager.Instance.SetAllGameTimeRate(1);
            this.UnregisterInput();
        }

        private void UnregisterInput()
        {
            // 切换类目
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.LastTerm.performed -= LastTerm_performed;
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.NextTerm.performed -= NextTerm_performed;

            //切换隐兽
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.NextGrid.performed -= NextGrid_performed;

            //切换对象
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.SwitchTarget.performed -= SwitchTarget_performed;




            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;

            

        }

        private void RegisterInput()
        {
            // 切换类目
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.LastTerm.performed += LastTerm_performed;
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.NextTerm.performed += NextTerm_performed;

            //切换隐兽
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.NextGrid.performed += NextGrid_performed;


            //切换对象
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.SwitchTarget.performed += SwitchTarget_performed;

            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;

        }





        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            UIMgr.PopPanel();
        }





        private void LastTerm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            CurrentFuctionTypeIndex = (CurrentFuctionTypeIndex + 2 - 1) % 2;
            this.Refresh();
        }

        private void NextTerm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            CurrentFuctionTypeIndex = (CurrentFuctionTypeIndex + 1) % 2;
            this.Refresh();
        }

        private void NextGrid_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            CurrentGridIndex = (CurrentGridIndex + 1) % Grids.Count;
            this.Refresh();
        }

        private void SwitchTarget_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {

            Debug.Log("SwitchTarget_performed!");
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
                Destroy(s);
            }
            foreach (var s in tempItemType.Values)
            {
                Destroy(s);
            }
            foreach (var s in tempUIItems)
            {
                Destroy(s);
            }
        }

        #endregion

        #region UI对象引用
         


        private TMPro.TextMeshProUGUI TopTitleText;

        private UIKeyTip KT_LastTerm;
        private Transform HiddenBeastResonanceTemplate;
        private Transform SongofSeaBeastsTemplate;
        private UIKeyTip KT_NextTerm;

        private List<Transform> Grids = new List<Transform>();
        private Transform Grid1, Grid2, Grid3, Grid4, Grid5;


        private UIKeyTip KT_NextGrid;
        #endregion

        public void Refresh()
        {
            if (ABJAProcessor == null || !ABJAProcessor.IsLoaded || !IsInit)
            {
                return;
            }


            





        }
        #endregion

        #region TextContent
        [System.Serializable]
        public struct ResonanceWheelPanel
        {
            public TextContent toptitle;
            public TextTip[] itemtype;
            public KeyTip lastterm;
            public KeyTip nextterm;
            public KeyTip nextgrid;
        }

        public static ResonanceWheelPanel PanelTextContent => ABJAProcessor.Datas;
        public static ML.Engine.ABResources.ABJsonAssetProcessor<ResonanceWheelPanel> ABJAProcessor;

        private void InitUITextContents()
        {
            if (ABJAProcessor == null)
            {
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<ResonanceWheelPanel>("JSON/TextContent/ResonanceWheel", "ResonanceWheelPanel", (datas) =>
                {
                    Refresh();
                    this.enabled = false;
                }, null, "UI共鸣轮Panel数据");
                ABJAProcessor.StartLoadJsonAssetData();
            }
            Debug.Log("1 "+ABJAProcessor.Datas.toptitle);
        }
        #endregion

        #region to-delete
        [Button("生成测试文件")]
        void GenTESTFILE()
        {
            List<ItemSpawner.ItemTableJsonData> datas = new List<ItemSpawner.ItemTableJsonData>();

            var itypes = Enum.GetValues(typeof(ML.Engine.InventorySystem.ItemType)).Cast<ML.Engine.InventorySystem.ItemType>().Where(e => (int)e > 0).ToArray();
            foreach (var itype in itypes)
            {
                int cnt = UnityEngine.Random.Range(50, 100);
                for (int i = 0; i < cnt; ++i)
                {
                    var data = new ItemSpawner.ItemTableJsonData();
                    // id
                    data.id = itype.ToString() + "_" + i;
                    // name
                    data.name = new TextContent();
                    data.name.Chinese = data.id;
                    data.name.English = data.id;
                    // type
                    data.type = "ResourceItem";
                    // sort
                    data.sort = i;
                    // itemtype
                    data.itemtype = itype;
                    // weight
                    data.weight = UnityEngine.Random.Range(1, 10);
                    // bcanstack
                    data.bcanstack = UnityEngine.Random.Range(1, 10) < 9;
                    // maxamount
                    data.maxamount = 999;
                    // texture2d
                    data.texture2d = "100001";
                    // worldobject
                    data.worldobject = "TESTWorldItem";
                    // description
                    data.description = "TTTTTTTTTTTTTTTTTTTTTTTT\nXXXXXXXXXXXXXXXXXXXXXXXX\nTTTTTTTTTTTTTTTTTTTTTTTT";
                    // effectsDescription
                    data.effectsDescription = "<color=#6FB502><b><sprite name=\"Triangle\" index=0 tint=1>+10%金币掉落\n<color=#6FB502><b><sprite name=\"Triangle\" index=0 tint=1>+10%攻击力持续300s</b></color>";
                    datas.Add(data);
                }
            }

            string json = JsonConvert.SerializeObject(datas.ToArray(), Formatting.Indented);

            System.IO.File.WriteAllText(Application.streamingAssetsPath + "/../../../t.json", json);
            Debug.Log("输出路径: " + System.IO.Path.GetFullPath(Application.streamingAssetsPath + "/../../../t.json"));
        }

        #endregion
    }

}
