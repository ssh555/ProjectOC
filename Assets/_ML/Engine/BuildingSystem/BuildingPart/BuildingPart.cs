using ML.Engine.InventorySystem.CompositeSystem;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ML.Engine.BuildingSystem.BuildingPart
{
    public class BuildingPart : MonoBehaviour, IBuildingPart, IComposition
    {
        #region IBuildingPart
        public string ID { get => BuildingManager.Instance.BPartTableDictOnClass[this.classification.ToString()].id; set => throw new Exception("���������ý������ID"); }

        [SerializeField, LabelText("����")]
        private BuildingPartClassification classification;
        public BuildingPartClassification Classification { get => classification; private set => classification = value; }

        [SerializeField, LabelText("ʵ��ID")]
        private string instanceID;
        public string InstanceID { get => instanceID; set => instanceID = value; }

        GameObject IBuildingPart.gameObject { get => this.gameObject;  }
        Transform IBuildingPart.transform { get => this.transform;  }
        public bool isFirstBuild { get; protected set; }= true;
        [SerializeField, LabelText("�ܷ����"), ReadOnly]
        private bool canPlaceInPlaceMode = true;
        public bool CanPlaceInPlaceMode 
        {
            get
            {
                //Debug.Log($"{this.canPlaceInPlaceMode} {this.AttachedArea != null} {this.AttachedSocket != null} {this.CheckCanInPlaceMode.Invoke(this)}");
                return this.canPlaceInPlaceMode && (this.AttachedArea != null || this.AttachedSocket != null) && (this.CheckCanInPlaceMode == null ? true : this.CheckCanInPlaceMode.Invoke(this));
            }
            set => canPlaceInPlaceMode = value; 
        }
        public event IBuildingPart.CheckMode CheckCanInPlaceMode;
        public event IBuildingPart.CheckMode CheckCanEdit;
        public event IBuildingPart.CheckMode CheckCanDestory;

        public virtual void OnChangePlaceEvent(Vector3 oldPos, Vector3 newPos)
        {
            if(isFirstBuild)
                isFirstBuild = false;
        }

        public bool CanEnterEditMode()
        {
            return CheckCanEdit == null || (CheckCanEdit != null && CheckCanEdit.Invoke(this));
        }

        public bool CanEnterDestoryMode()
        {
            return CheckCanDestory == null || (CheckCanDestory != null && CheckCanDestory.Invoke(this));
        }

        [SerializeField, LabelText("ģʽ")]
        private BuildingMode mode;
        public BuildingMode Mode
        {
            get => mode;
            set
            {
                mode = value;
                this.SetColliderTrigger((mode == BuildingMode.Place || mode == BuildingMode.Destroy || mode == BuildingMode.Edit));
                if (mode == BuildingMode.Edit || mode == BuildingMode.Place)
                {
                    this.canPlaceInPlaceMode = true;
                }
                else
                {
                    this.canPlaceInPlaceMode = false;
                }
                
                Material mat = BuildingManager.Instance.GetBuldingMat(mode);
                if(mat != null)
                {
                    foreach (var kv in this.rowMat)
                    {
                        kv.Key.sharedMaterial = mat;
                    }
                }
                else
                {
                    foreach(var kv in this.rowMat)
                    {
                        kv.Key.sharedMaterials = kv.Value;
                    }
                }
            }
        }

        [SerializeField, HideInInspector]
        private Quaternion baseRotation = Quaternion.identity;
        public Quaternion BaseRotation { get => baseRotation; private set => baseRotation = value; }

        [SerializeField]
        private Quaternion rotOffset = Quaternion.identity;
        public Quaternion RotOffset { get => rotOffset; set => rotOffset = value; }
        private BuildingArea.BuildingArea attachedArea;
        public BuildingArea.BuildingArea AttachedArea
        {
            get => this.attachedArea;
            set
            {
                if (value == null)
                {
                    this.attachedArea = null;
                }
                else if (value != null)
                {
                    if (value.CheckAreaTypeMatch(this))
                    {
                        this.attachedArea = value;
                    }
                    else
                    {
                        this.attachedArea = null;
                    }
                }
            }
        }
        private BuildingSocket.BuildingSocket attachedSocket;
        public BuildingSocket.BuildingSocket AttachedSocket
        {
            get => this.attachedSocket;
            set
            {
                if(value == null)
                {
                    this.attachedSocket = null;
                }
                else if(value != null)
                {
                    if (this.ActiveSocket.CheckMatch(value))
                    {
                        this.attachedSocket = value;
                    }
                    else
                    {
                        this.attachedSocket = null;
                    }
                }
            }
        }
        
        [SerializeField]
        private int activeSocketIndex;
        public BuildingSocket.BuildingSocket ActiveSocket { 
            get => this.OwnedSocketList[activeSocketIndex];
            set 
            {
                activeSocketIndex = this.OwnedSocketList.IndexOf(value);
            }
        }
        public List<BuildingSocket.BuildingSocket> OwnedSocketList { get; private set; }
        public int lastTriggerFrameCount { get; set; }
        public BuildingMode tmpTriggerMode { get; set; }


        public void AlternativeActiveSocket()
        {
            ActiveSocket.OnDisactive();
            this.activeSocketIndex = (this.activeSocketIndex + 1) % this.OwnedSocketList.Count;
            while(!this.ActiveSocket.IsActiveSocket)
            {
                this.activeSocketIndex = (this.activeSocketIndex + 1) % this.OwnedSocketList.Count;
            }
            ActiveSocket.OnActive();
        }

        public BuildingCopiedMaterial GetCopiedMaterial()
        {
            BuildingCopiedMaterial mat = new BuildingCopiedMaterial();
            var p = this.GetComponent<Renderer>();
            if(p)
            {
                mat.ParentMat = rowMat[p];

            }
            for (int i = this.transform.childCount - 1; i >= 0; --i)
            {
                var renderer = this.transform.GetChild(i).GetComponent<Renderer>();
                if (renderer)
                {
                    mat.ChildrenMat.Add(i, rowMat[renderer]);
                }
            }
            return mat;
        }

        public void SetCopiedMaterial(BuildingCopiedMaterial mat)
        {
            if (mat.ParentMat != null)
            {
                this.GetComponent<Renderer>().sharedMaterials = mat.ParentMat;
            }
            if(mat.ChildrenMat != null)
            {
                foreach (var kv in mat.ChildrenMat)
                {
                    this.transform.GetChild(kv.Key).GetComponent<Renderer>().sharedMaterials = kv.Value;
                }
            }

            this.rowMat.Clear();
            foreach (var renderer in this.GetComponentsInChildren<Renderer>())
            {
                this.rowMat.Add(renderer, renderer.sharedMaterials);
            }
        }

        #endregion

        /// <summary>
        /// ԭ������
        /// </summary>
        private Dictionary<Renderer, Material[]> rowMat;


        protected virtual void Awake()
        {
            this.rowMat = new Dictionary<Renderer, Material[]>();
            foreach(var renderer in this.GetComponentsInChildren<Renderer>())
            {
                this.rowMat.Add(renderer, renderer.sharedMaterials);
            }

            this.OwnedSocketList = new List<BuildingSocket.BuildingSocket>();
            this.OwnedSocketList.AddRange(this.GetComponentsInChildren<BuildingSocket.BuildingSocket>());

            // ָ���һ�����л���Socket
            this.activeSocketIndex = 0;
            while (!this.ActiveSocket.IsActiveSocket)
            {
                this.activeSocketIndex = (this.activeSocketIndex + 1) % this.OwnedSocketList.Count;
            }

            //this.ActiveSocket = this.OwnedSocketList[0];
            this.enabled = true;
        }

        protected virtual void Start()
        {
            this.enabled = false;
        }

        private void SetColliderTrigger(bool isTrigger)
        {
            foreach(var col in this.GetComponentsInChildren<Collider>())
            {
                if(col.gameObject.layer != 8)
                {
                    if(col is MeshCollider)
                    {
                        MeshCollider mcol = col as MeshCollider;
                        // ��ֵ˳���ܵߵ�
                        if(isTrigger)
                        {
                            mcol.convex = true;
                            mcol.isTrigger = true;
                        }
                        else
                        {
                            mcol.isTrigger = false;
                            mcol.convex = false;
                        }
                    }
                    else
                    {
                        col.isTrigger = isTrigger;
                    }
                }
            }
            foreach (var socket in this.GetComponentsInChildren<BuildingSocket.BuildingSocket>())
            {
                socket.enabled = !isTrigger;
            }
            foreach (var area in this.GetComponentsInChildren<BuildingArea.BuildingArea>())
            {
                area.enabled = !isTrigger;
            }

            if (isTrigger == true && this.GetComponent<Rigidbody>() == null)
            {
                this.gameObject.AddComponent<Rigidbody>().isKinematic = true;
            }
            else if(isTrigger == false)
            {
                Manager.GameManager.DestroyObj(this.GetComponent<Rigidbody>());
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            (this as IBuildingPart).CheckTriggerStay(other);
        }

        public void OnTriggerStay(Collider other)
        {
            (this as IBuildingPart).CheckTriggerStay(other);
        }
        public void OnTriggerExit(Collider other)
        {
            if (this.Mode == BuildingMode.Place || this.Mode == BuildingMode.Destroy || this.Mode == BuildingMode.Edit)
            {
                this.CanPlaceInPlaceMode = true;
                this.Mode = this.CanPlaceInPlaceMode ? this.tmpTriggerMode : BuildingMode.Destroy;
            }
        }

#if UNITY_EDITOR
        [Button("�����ʲ�����������"), PropertyOrder(-1), ShowIf("IsPrefab")]
        private void ChangeCategoryFromName()
        {
            Classification = new BuildingPartClassification(this.name);
        }

        public bool IsPrefab()
        {
            var type = UnityEditor.PrefabUtility.GetPrefabAssetType(this.gameObject);
            var status = UnityEditor.PrefabUtility.GetPrefabInstanceStatus(this.gameObject);
            if (type != UnityEditor.PrefabAssetType.NotAPrefab && status == UnityEditor.PrefabInstanceStatus.NotAPrefab)
            {
                return true;
            }
            return false;
        }
#endif

        public virtual void OnBPartDestroy()
        {

        }

        //private void OnDrawGizmosSelected()
        //{
        //    foreach(var col in this.GetComponentsInChildren<BuildingSocket.BuildingSocket>())
        //    {
        //        Extension.GizmosExtension.DrawWireCollider(col.GetComponent<Collider>(), Color.yellow);
        //    }
        //    foreach (var col in this.GetComponentsInChildren<BuildingArea.BuildingArea>())
        //    {
        //        Extension.GizmosExtension.DrawWireCollider(col.GetComponent<Collider>(), Color.blue);
        //    }
        //}
    }



}
