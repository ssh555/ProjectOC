using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InventorySystem;
using ML.Engine.TextContent;
using ML.Engine.Timer;
using Newtonsoft.Json;
using ProjectOC.TechTree.UI;
using ProjectOC.WorkerNS;
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
    public class ResonanceWheelUI : ML.Engine.UI.UIBasePanel,ITickComponent
    {

        public IInventory inventory;

        
        

        #region Input
        /// <summary>
        /// ����Drop��Destroy������ӦCancel
        /// ������Ӧ��Destroy����Ϊtrue
        /// Cancel�Ͳ���ӦDrop �� ����
        /// </summary>

        #endregion

        #region Unity
        public bool IsInit = false;
        private void Start()
        {
            InitUITextContents();

            //exclusivePart
            exclusivePart = this.transform.Find("ExclusivePart");

            // TopTitle
            TopTitleText = this.transform.Find("TopTitle").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();

            //FuctionType

            var content = this.transform.Find("FunctionType").Find("Content");
            KT_LastTerm = new UIKeyTip();
            KT_LastTerm.img = content.Find("KT_LastTerm").Find("Image").GetComponent<UnityEngine.UI.Image>();
            KT_LastTerm.keytip = KT_LastTerm.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();

            HiddenBeastResonanceTemplate = content.Find("HiddenBeastResonanceContainer").Find("HiddenBeastResonanceTemplate");
            SongofSeaBeastsTemplate = content.Find("SongofSeaBeastsContainer").Find("SongofSeaBeastsTemplate");

            KT_NextTerm = new UIKeyTip();
            KT_NextTerm.img = content.Find("KT_NextTerm").Find("Image").GetComponent<UnityEngine.UI.Image>();
            KT_NextTerm.keytip = KT_NextTerm.img.transform.Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();

            //Ring
            var ringcontent = exclusivePart.Find("Ring").Find("Viewport").Find("Content");
            KT_NextGrid = new UIKeyTip();
            KT_NextGrid.keytip= ringcontent.Find("SelectKey").GetComponent<TMPro.TextMeshProUGUI>();
            var ring = ringcontent.Find("Ring");
            Grid1 = ring.Find("Grid1");
            Grid2 = ring.Find("Grid2");
            Grid3 = ring.Find("Grid3");
            Grid4 = ring.Find("Grid4");
            Grid5 = ring.Find("Grid5");
            Grids.Add(new RingGrid(Grid1.transform));
            Grids.Add(new RingGrid(Grid2.transform));
            Grids.Add(new RingGrid(Grid3.transform));
            Grids.Add(new RingGrid(Grid4.transform));
            Grids.Add(new RingGrid(Grid5.transform));


            IsInit = true;
            Refresh();
        }

        #endregion

        #region Tick
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }

        public void Tick(float deltatime)
        {
            Debug.Log("Tick");
        }

        #endregion




        #region Override
        public override void OnEnter()
        {
            base.OnEnter();
            this.Enter();
            ML.Engine.Manager.GameManager.Instance.TickManager.RegisterTick(0, this);
        }

        public override void OnExit()
        {
            ML.Engine.Manager.GameManager.Instance.TickManager.UnregisterTick(this);

            base.OnExit();

            Debug.Log("1 " + ML.Engine.Manager.GameManager.Instance.UIManager.GetTopUIPanel());
            this.Exit();
            ClearTemp();
        }

        public override void OnPause()
        {
            ML.Engine.Manager.GameManager.Instance.TickManager.UnregisterTick(this);
            //setfalse ��ui�Ķ��в���
            exclusivePart?.gameObject.SetActive(false);

            //������ui���е�����
            //�л�����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.NextGrid.performed -= NextGrid_performed;


            //�л�����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.SwitchTarget.performed -= SwitchTarget_performed;

            // ����
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;

            //base.OnPause();
            //this.Exit();
        }

        public override void OnRecovery()
        {
            ML.Engine.Manager.GameManager.Instance.TickManager.RegisterTick(0, this);


            //setfalse ��ui�Ķ��в���
            if (exclusivePart != null)
                exclusivePart.gameObject.SetActive(true);

            //�ָ���ui���е�����
            //�л�����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.NextGrid.performed += NextGrid_performed;


            //�л�����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.SwitchTarget.performed += SwitchTarget_performed;

            // ����
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;

            //base.OnRecovery();
            this.Enter();
        }


        private void OnDestroy()
        {
            ClearTemp();
            (this as ITickComponent).DisposeTick();
        }

        #endregion

        #region Internal

        public class RingGrid
        {
            public Sprite sprite;//������ʾ��ͼ
            public bool isNull;//�Ƿ�Ϊ��
            public bool isResonating;//�Ƿ������
            public Worker worker;//��Ӧ������
            public Transform transform;

            public RingGrid(Sprite sprite, bool isNull, bool isResonating, Worker worker,Transform transform)
            {
                this.sprite = sprite;
                this.isNull = isNull;
                this.isResonating = isResonating;
                this.worker = worker;
                this.transform = transform;
            }

            public RingGrid(Transform transform)
            {
                this.sprite = null;
                this.isNull = true;
                this.isResonating = false;
                this.worker = null;
                this.transform = transform;
            }
        }




        private void Enter()
        {
            this.RegisterInput();
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.Enable();
            

            this.Refresh();
        }

        private void Exit()
        {
            this.UnregisterInput();
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.Disable();
            
        }

        private void UnregisterInput()
        {
            // �л���Ŀ
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.LastTerm.performed -= LastTerm_performed;
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.NextTerm.performed -= NextTerm_performed;

            //�л�����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.NextGrid.performed -= NextGrid_performed;

            //�л�����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.SwitchTarget.performed -= SwitchTarget_performed;

            //��ʼ����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.SwitchTarget.performed -= StartResonance_performed;

            // ����
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;

            

        }

        private void RegisterInput()
        {
            // �л���Ŀ
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.LastTerm.performed += LastTerm_performed;
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.NextTerm.performed += NextTerm_performed;

            //�л�����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.NextGrid.performed += NextGrid_performed;


            //�л�����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.SwitchTarget.performed += SwitchTarget_performed;

            //��ʼ����
            ProjectOC.Input.InputManager.PlayerInput.ResonanceWheelUI.SwitchTarget.performed += StartResonance_performed;

            // ����
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;

        }



        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            Debug.Log("main Back_performed");
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
            Debug.Log("253453 " + Grids[0]);
            Debug.Log("123412 "+Grids.Count);
            CurrentGridIndex = (CurrentGridIndex + 1) % Grids.Count;
            this.Refresh();
        }

        private void SwitchTarget_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            var panel = GameObject.Instantiate(resonanceWheel_Sub2);
            panel.transform.SetParent(this.transform.parent, false);

            
            ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);
            //ActiveSubUI();
            Debug.Log("SwitchTarget_performed!");
            
        }

        private void StartResonance_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            //��鱳��

            //�ܷ�ɹ��ϳ� �п�



            Debug.Log("StartResonance_performed!");

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

        #region UI��������
        public ResonanceWheel_sub1 resonanceWheel_Sub1;//��ϸ���޽��� 
        public ResonanceWheel_sub2 resonanceWheel_Sub2;//���ѡ�����޽��� 

        public Transform exclusivePart;//��ui���в���

        



        private TMPro.TextMeshProUGUI TopTitleText;

        private UIKeyTip KT_LastTerm;
        private Transform HiddenBeastResonanceTemplate;
        private Transform SongofSeaBeastsTemplate;
        private int CurrentFuctionTypeIndex = 0;//0ΪHBR 1ΪSSB
        private UIKeyTip KT_NextTerm;

        [ShowInInspector]
        private List<RingGrid> Grids = new List<RingGrid>();
        private Transform Grid1, Grid2, Grid3, Grid4, Grid5;
        
        private int CurrentGridIndex = 0;//0��4

        private UIKeyTip KT_NextGrid;
        #endregion

        public void Refresh()
        {
            if (ABJAProcessor == null || !ABJAProcessor.IsLoaded || !IsInit)
            {
                return;
            }


            TopTitleText.text = PanelTextContent.toptitle;

            #region FunctionType
            this.KT_LastTerm.ReWrite(PanelTextContent.lastterm);
            GameObject HBR = HiddenBeastResonanceTemplate.Find("Selected").gameObject;
            GameObject SSB = SongofSeaBeastsTemplate.Find("Selected").gameObject;

            if (CurrentFuctionTypeIndex == 0) 
            {
                HBR.SetActive(false);
                SSB.SetActive(true);
            }
            else if(CurrentFuctionTypeIndex == 1)
            {
                HBR.SetActive(true);
                SSB.SetActive(false);
            }

            this.KT_NextTerm.ReWrite(PanelTextContent.nextterm);
            #endregion

            #region Ring
            this.KT_NextGrid.ReWrite(PanelTextContent.nextgrid);

            for (int i = 0; i < Grids.Count; i++) 
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


            #endregion


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
                }, null, "UI������Panel����");
                ABJAProcessor.StartLoadJsonAssetData();
            }
            Debug.Log("1 "+ABJAProcessor.Datas.toptitle);
        }
        #endregion

        #region to-delete
        [Button("���ɲ����ļ�")]
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
                    data.effectsDescription = "<color=#6FB502><b><sprite name=\"Triangle\" index=0 tint=1>+10%��ҵ���\n<color=#6FB502><b><sprite name=\"Triangle\" index=0 tint=1>+10%����������300s</b></color>";
                    datas.Add(data);
                }
            }

            string json = JsonConvert.SerializeObject(datas.ToArray(), Formatting.Indented);

            System.IO.File.WriteAllText(Application.streamingAssetsPath + "/../../../t.json", json);
            Debug.Log("���·��: " + System.IO.Path.GetFullPath(Application.streamingAssetsPath + "/../../../t.json"));
        }

        #endregion
    }

}
