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

        public void Init()
        {
            Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<RenderTexture>("Prefab_BaseUIPrefab_UICameraImage/RenderTex2D_UICameraImage_RT.renderTexture").Completed += (handle) =>
            {
                this.texture = handle.Result;

                cameraParent = new GameObject("UICameraImageRoot");
                cameraParent.transform.position = cameraSpawnPoint;

                GameObject cameraObject = new GameObject("UICamera");
                cameraObject.transform.SetParent(cameraParent.transform);
                cameraObject.transform.localPosition = Vector3.zero;
                cameraObject.transform.localRotation = Quaternion.identity;

                uiCamera = cameraObject.AddComponent<Camera>();
                uiCamera.clearFlags = CameraClearFlags.Color;
                uiCamera.backgroundColor = Color.black;
                uiCamera.targetTexture = texture;
                uiCamera.orthographic = false;

                RawImage rawImage = transform.GetComponentInChildren<RawImage>();
                rawImage.texture = uiCamera.targetTexture;
                this.isInit = true;
            };
        }

        public void LookAtGameObject(GameObject go)
        {
            if (go == null) return;
            if (currentObjectBeingObserved != null)
            {
                GameManager.DestroyObj(currentObjectBeingObserved);
            }

            currentObjectBeingObserved = go;
            currentObjectBeingObserved.transform.SetParent(cameraParent.transform);
            currentObjectBeingObserved.transform.localPosition = Vector3.zero;
            currentObjectBeingObserved.transform.localRotation = Quaternion.identity;

            Vector3 direction = currentObjectBeingObserved.transform.position - uiCamera.transform.position;
            uiCamera.transform.rotation = Quaternion.LookRotation(-direction, Vector3.up);

            uiCamera.transform.position = currentObjectBeingObserved.transform.position - uiCamera.transform.forward * distanceFromObject;
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


