using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.UI;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;

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
                try
                {
                    GameObject gameObject = CurrentInteraction.gameObject;
                }
                catch (UnityEngine.MissingReferenceException)
                {
                    CurrentInteraction = null;
                    return;
                }

                if (hits == null || hits.Length == 0)
                {
                    CurrentInteraction.OnSelectedExit(this);
                    CurrentInteraction = null;
                    uiKeyTip.img.transform.parent.gameObject.SetActive(false);
                    return;
                }
                uiKeyTip.img.transform.parent.gameObject.SetActive(true);

                KeyTip keyTip = GetKeyTip(CurrentInteraction.InteractType);
                InputAction inputAction = GameManager.Instance.InputManager.GetInputAction((keyTip.keymap.ActionMapName, keyTip.keymap.ActionName));

                uiKeyTip.keytip.text = GameManager.Instance.InputManager.GetInputActionBindText(inputAction);
                uiKeyTip.description.text = keyTip.description.GetText();

                // ����������ת��Ϊ��Ļ����
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(CurrentInteraction.gameObject.transform.position + CurrentInteraction.PosOffset);

                // �����Ļ���곬������Ļ��Χ��������������Ļ��Ե
                screenPosition.x = Mathf.Clamp(screenPosition.x, 0, Screen.width);
                screenPosition.y = Mathf.Clamp(screenPosition.y, 0, Screen.height);

                // ����UIԪ�ص�λ��
                uiKeyTip.img.transform.parent.position = screenPosition;

                // ȷ�Ͻ���
                if (Input.InputManager.Instance.Common.Common.Confirm.WasPressedThisFrame())
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
        [LabelText("����")]
        public LayerMask layerMask;
        [LabelText("������")]
        public float range = 5f;

        /// <summary>
        /// ��ǰѡ�еĿɽ�����
        /// </summary>
        public IInteraction CurrentInteraction { get; protected set; }
        public void SetInteractionNull()
        {
            CurrentInteraction = null;
        }
        public void Disable()
        {
            Manager.GameManager.Instance.TickManager.UnregisterTick(this);

            if(!this.IsDestroyed() && uiKeyTip.img)
            {
                uiKeyTip.img.transform.parent.gameObject.SetActive(false);
            }
        }
        public void Enable()
        {
            Manager.GameManager.Instance.TickManager.RegisterTick(0, this);
        }
        #endregion

        #region Unity
        private void Start()
        {
            // ��������
            InitUITextContents();

            Manager.GameManager.Instance.TickManager.RegisterTick(0, this);
        }

        private void OnDestroy()
        {
            (this as Timer.ITickComponent).DisposeTick();
            if(Manager.GameManager.Instance != null)
                Manager.GameManager.Instance.ABResourceManager.ReleaseInstance(ktHandle);
        }
        #endregion

        #region UI
        /// <summary>
        /// keyname -> key -> IInteraction.InteractType
        /// </summary>
        private Dictionary<string, TextContent.KeyTip> STATIC_KTDict = new Dictionary<string, TextContent.KeyTip>();

        private KeyTip GetKeyTip(string key)
        {
            if(STATIC_KTDict.ContainsKey(key))
            {
                return STATIC_KTDict[key];
            }
            return STATIC_KTDict["Interaction"];
        }

        public ML.Engine.ABResources.ABJsonAssetProcessor<TextContent.KeyTip[]> ABJAProcessor;

        public void InitUITextContents()
        {
            ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<TextContent.KeyTip[]>("OC/Json/TextContent/InteractSystem", "InteractKeyTip", (datas) =>
            {
                foreach (var tip in datas)
                {
                    STATIC_KTDict.Add(tip.keyname, tip);
                }

            }, "�������������ʾ");
            ABJAProcessor.StartLoadJsonAssetData();

            OnLoadOver();
        }

        private void OnLoadOver()
        {
            Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("ML/InteractSystem/UI/InteractKeyTip.prefab").Completed += (handle) =>
             {
                 this.ktHandle = handle;
                 var keyTipInstance = handle.Result;
                 uiKeyTip = new UIKeyTip();
                 uiKeyTip.img = keyTipInstance.transform.Find("Image").GetComponent<UnityEngine.UI.Image>();
                 uiKeyTip.keytip = uiKeyTip.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
                 uiKeyTip.description = uiKeyTip.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
                 uiKeyTip.img.transform.parent.gameObject.SetActive(false);

                 // TODO
                 uiKeyTip.img.transform.parent.SetParent(Manager.GameManager.Instance.UIManager.GetCanvas.transform);

                 // ���� Mono
                 this.enabled = false;
             };
        }

        private AsyncOperationHandle ktHandle;
        private TextContent.UIKeyTip uiKeyTip;

        #endregion
    }
}
