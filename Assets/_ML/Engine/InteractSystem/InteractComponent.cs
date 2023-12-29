using ML.Engine.TextContent;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ML.Engine.InteractSystem
{
    public class InteractComponent : MonoBehaviour, Timer.ITickComponent
    {
        #region ITickComponent
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }
        public virtual void Tick(float deltatime)
        {
            // 射线检测可交互物
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            RaycastHit[] hits = Physics.RaycastAll(ray, range, layerMask);
            foreach (RaycastHit hit in hits)
            {
                var interact = hit.collider.gameObject.GetComponentInParent<IInteraction>();
                if(interact != null)
                {
                    if(CurrentInteraction == interact)
                    {
                        break;
                    }
                    if(CurrentInteraction != null)
                    {
                        CurrentInteraction.OnSelectedExit(this);
                    }
                    CurrentInteraction = interact;
                    CurrentInteraction.OnSelectedEnter(this);
                    break;
                }
            }


            // 检测到时显示UITip
            if (CurrentInteraction != null)
            {
                if (hits == null || hits.Length == 0)
                {
                    CurrentInteraction.OnSelectedExit(this);
                    CurrentInteraction = null;
                    uiKeyTip.img.transform.parent.gameObject.SetActive(false);
                    return;
                }
                uiKeyTip.img.transform.parent.gameObject.SetActive(true);
                uiKeyTip.ReWrite(GetKeyTip(CurrentInteraction.InteractType));

                // 将世界坐标转换为屏幕坐标
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(CurrentInteraction.gameObject.transform.position + CurrentInteraction.PosOffset);

                // 如果屏幕坐标超出了屏幕范围，将其设置在屏幕边缘
                screenPosition.x = Mathf.Clamp(screenPosition.x, 0, Screen.width);
                screenPosition.y = Mathf.Clamp(screenPosition.y, 0, Screen.height);

                // 更新UI元素的位置
                uiKeyTip.img.transform.parent.position = screenPosition;

                // 确认交互
                if (Input.InputManager.Instance.Common.Common.Comfirm.WasPressedThisFrame())
                {
                    this.CurrentInteraction.Interact(this);
                }
            }
            else
            {
                if(uiKeyTip.img != null)
                {
                    uiKeyTip.img.transform.parent.gameObject.SetActive(false);
                }
            }
        }
        #endregion

        #region Interact
        [LabelText("检测层")]
        public LayerMask layerMask;
        [LabelText("检测距离")]
        public float range = 5f;

        /// <summary>
        /// 当前选中的可交互物
        /// </summary>
        public IInteraction CurrentInteraction { get; protected set; }
        public void SetInteractionNull()
        {
            CurrentInteraction = null;
        }
        public void Disable()
        {
            Manager.GameManager.Instance.TickManager.UnregisterTick(this);
        }
        public void Enable()
        {
            Manager.GameManager.Instance.TickManager.RegisterTick(0, this);
        }
        #endregion

        #region Unity
        private void Start()
        {
            // 载入数据
            InitUITextContents();

            Manager.GameManager.Instance.TickManager.RegisterTick(0, this);
        }

        private void OnDestroy()
        {
            (this as Timer.ITickComponent).DisposeTick();
        }
        #endregion

        #region UI
        /// <summary>
        /// keyname -> key -> IInteraction.InteractType
        /// </summary>
        private static Dictionary<string, TextContent.KeyTip> STATIC_KTDict = new Dictionary<string, TextContent.KeyTip>();

        private KeyTip GetKeyTip(string key)
        {
            if(STATIC_KTDict.ContainsKey(key))
            {
                return STATIC_KTDict[key];
            }
            return STATIC_KTDict["Interaction"];
        }

        public static ML.Engine.ABResources.ABJsonAssetProcessor<TextContent.KeyTip[]> ABJAProcessor;

        public void InitUITextContents()
        {
            if (ABJAProcessor == null)
            {
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<TextContent.KeyTip[]>("JSON/TextContent/InteractSystem", "InteractKeyTip", (datas) =>
                {
                    foreach (var tip in datas)
                    {
                        STATIC_KTDict.Add(tip.keyname, tip);
                    }

                }, null, "交互组件按键提示");
                ABJAProcessor.StartLoadJsonAssetData();
            }
            StartCoroutine(OnLoadOver());
        }

        private IEnumerator OnLoadOver()
        {
            var abmgr = Manager.GameManager.Instance.ABResourceManager;

            // 载入 keyTipPrefab
            var crequest = abmgr.LoadLocalABAsync("UI/InteractSystem", null, out var ab);
            yield return crequest;
            if (crequest != null)
            {
                ab = crequest.assetBundle;
            }
            var keyTipPrefab = Instantiate(ab.LoadAsset<GameObject>("InteractKeyTip"));
            uiKeyTip = new UIKeyTip();
            uiKeyTip.img = keyTipPrefab.transform.Find("Image").GetComponent<UnityEngine.UI.Image>();
            uiKeyTip.keytip = uiKeyTip.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            uiKeyTip.description = uiKeyTip.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            uiKeyTip.img.transform.parent.gameObject.SetActive(false);

            uiKeyTip.img.transform.parent.SetParent(GameObject.Find("Canvas").transform);

            while(!ABJAProcessor.IsLoaded)
            {
                yield return null;
            }
            // 禁用 Mono
            this.enabled = false;
        }

        private TextContent.UIKeyTip uiKeyTip;

        #endregion
    }
}
