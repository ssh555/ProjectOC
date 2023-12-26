using ML.Engine.TextContent;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static ProjectOC.TechTree.TechTreeManager;

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
            // ���߼��ɽ�����
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


            // ��⵽ʱ��ʾUITip
            if (CurrentInteraction != null)
            {
                if (hits == null || hits.Length == 0)
                {
                    CurrentInteraction.OnSelectedExit(this);
                    CurrentInteraction = null;
                    return;
                }
                uiKeyTip.img.transform.parent.gameObject.SetActive(true);
                uiKeyTip.ReWrite(GetKeyTip(CurrentInteraction.InteractType));
                var screenPosition = Camera.main.WorldToScreenPoint(CurrentInteraction.gameObject.transform.position + CurrentInteraction.PosOffset);
                if (screenPosition.x < 0)
                {
                    screenPosition.x = 0;
                }
                else if (screenPosition.x > Screen.width)
                {
                    screenPosition.x = Screen.width;
                }

                if (screenPosition.y < 0)
                {
                    screenPosition.y = 0;
                }
                else if (screenPosition.y > Screen.height)
                {
                    screenPosition.y = Screen.height;
                }

                Vector2 localPosition; // UI����
                RectTransformUtility.ScreenPointToLocalPointInRectangle(uiKeyTip.img.transform.parent.parent as RectTransform, screenPosition, null, out localPosition);

                uiKeyTip.img.transform.parent.GetComponent<RectTransform>().anchoredPosition = localPosition;

                // ȷ�Ͻ���
                if (Input.InputManager.Instance.Common.Common.Comfirm.WasPressedThisFrame())
                {
                    this.CurrentInteraction.Interact(this);
                }
            }


        }
        #endregion

        #region Interact
        [LabelText("����")]
        public LayerMask layerMask;
        [LabelText("������")]
        public float range = 5f;

        /// <summary>
        /// ��ǰѡ�еĿɽ�����
        /// </summary>
        public IInteraction CurrentInteraction { get; protected set; }
        #endregion

        #region Unity
        private void Start()
        {
            // ��������
            StartCoroutine(InitUITextContents());

            Manager.GameManager.Instance.TickManager.RegisterTick(0, this);
        }

        private void OnDestroy()
        {
            (this as Timer.ITickComponent).DisposeTick();
        }
        #endregion

        #region UI
        public const string TextContentABPath = "JSON/TextContent/InteractSystem";
        public const string ABName = "InteractKeyTip";
        /// <summary>
        /// keyname -> key -> IInteraction.InteractType
        /// </summary>
        private Dictionary<string, TextContent.KeyTip> KTDict = new Dictionary<string, TextContent.KeyTip>();

        private KeyTip GetKeyTip(string key)
        {
            if(KTDict.ContainsKey(key))
            {
                return KTDict[key];
            }
            return KTDict["Interaction"];
        }

        private IEnumerator InitUITextContents()
        {
            while (ML.Engine.Manager.GameManager.Instance.ABResourceManager == null)
            {
                yield return null;
            }
#if UNITY_EDITOR
            float startT = Time.realtimeSinceStartup;
#endif
            var abmgr = ML.Engine.Manager.GameManager.Instance.ABResourceManager;
            AssetBundle ab;
            var crequest = abmgr.LoadLocalABAsync(TextContentABPath, null, out ab);
            yield return crequest;
            if (crequest != null)
            {
                ab = crequest.assetBundle;
            }

            var request = ab.LoadAssetAsync<TextAsset>(ABName);
            yield return request;
            TextContent.KeyTip[] tips = JsonConvert.DeserializeObject<TextContent.KeyTip[]>((request.asset as TextAsset).text);
            foreach (var tip in tips)
            {
                this.KTDict.Add(tip.keyname, tip);
            }

            // ���� Mono
            this.enabled = false;

            // ���� keyTipPrefab
            crequest = abmgr.LoadLocalABAsync("UI/InteractSystem", null, out ab);
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

#if UNITY_EDITOR
            Debug.Log("InitUITextContents cost time: " + (Time.realtimeSinceStartup - startT));
#endif
        }

        private TextContent.UIKeyTip uiKeyTip;

        #endregion
    }
}
