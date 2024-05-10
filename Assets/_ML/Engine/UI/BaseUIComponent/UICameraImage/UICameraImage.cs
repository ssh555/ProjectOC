using ML.Engine.Manager;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace ML.Engine.UI
{
    public class UICameraImage : UIBehaviour,IDragHandler
    {
        [LabelText("摄像机生成位置")]
        public Vector3 cameraSpawnPoint = new Vector3(1000, 1000, 1000);
        [LabelText("摄像机与模型之间的距离")]
        public float distanceFromObject = 10f;
        [LabelText("旋转模型速度")]
        public float speed = 2f;

        [ShowInInspector]
        private RenderTexture texture;
        [ShowInInspector]
        private GameObject cameraParent;
        [ShowInInspector]
        private GameObject currentObjectBeingObserved;
        [ShowInInspector]
        private Camera uiCamera;
        [ShowInInspector]
        private bool isInit = false;

        private string LayerName = "UICamera";
        //public LayerMask layerMask; 

        public void Init()
        {
            Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<RenderTexture>("BaseUIPrefab/UICameraImage/RenderTex2D_UICameraImage_RT.renderTexture").Completed += (handle) =>
            {
                Init(handle.Result);
            };
        }

        public void Init(RenderTexture _rt)
        {
            this.texture = _rt;
            cameraParent = new GameObject("UICameraImageRoot");
            cameraParent.transform.position = cameraSpawnPoint;

            GameObject cameraObject = new GameObject("UICamera");
            cameraObject.transform.SetParent(cameraParent.transform);
            cameraObject.transform.localPosition = Vector3.zero;
            cameraObject.transform.localRotation = Quaternion.identity;

            uiCamera = cameraObject.AddComponent<Camera>();
            uiCamera.cullingMask = 1 << (LayerMask.NameToLayer(LayerName));
            uiCamera.clearFlags = CameraClearFlags.Color;
            uiCamera.backgroundColor = Color.grey;
            uiCamera.targetTexture = texture;
            uiCamera.orthographic = false;

            RawImage rawImage = transform.GetComponentInChildren<RawImage>();
            rawImage.texture = uiCamera.targetTexture;
            
            currentObjectBeingObserved = new GameObject("FouceObject");
            currentObjectBeingObserved.transform.SetParent(cameraParent.transform);
            currentObjectBeingObserved.transform.localPosition = Vector3.zero;
            currentObjectBeingObserved.transform.localRotation = Quaternion.identity;
            
            this.isInit = true;
        }
        
        public void LookAtGameObject(GameObject go,bool modeLayer = true)
        {
            if (go == null) return;

            if(currentObjectBeingObserved != null)
            {
                GameManager.DestroyObj(currentObjectBeingObserved);
            }

            currentObjectBeingObserved = go;
            currentObjectBeingObserved.transform.SetParent(cameraParent.transform);
            if (modeLayer)
            {
                ModeLayer(currentObjectBeingObserved.transform,LayerName);
            }

            currentObjectBeingObserved.transform.localPosition = Vector3.zero;
            currentObjectBeingObserved.transform.localRotation = Quaternion.identity;
            cameraParent.transform.position = go.transform.position;
            
            Vector3 direction = currentObjectBeingObserved.transform.position - uiCamera.transform.position;
            uiCamera.transform.rotation = Quaternion.LookRotation(-direction, Vector3.up);

            uiCamera.transform.position = currentObjectBeingObserved.transform.position - uiCamera.transform.forward * distanceFromObject;

            void ModeLayer(Transform _transform, string _layerValue)
            {
                _transform.gameObject.layer = LayerMask.NameToLayer(_layerValue);
                foreach (Transform _transf in _transform)
                {
                    ModeLayer(_transf, _layerValue);
                }
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (this.currentObjectBeingObserved == null) return;
            float x = speed * eventData.delta.y * Time.deltaTime;
            float y = speed * -eventData.delta.x * Time.deltaTime;
            this.currentObjectBeingObserved.transform.Rotate(x, y, 0, Space.World);
        }

        public void DisableUICameraImage()
        {
            base.OnDestroy();
            GameManager.DestroyObj(this.cameraParent);
        }
    }
}


