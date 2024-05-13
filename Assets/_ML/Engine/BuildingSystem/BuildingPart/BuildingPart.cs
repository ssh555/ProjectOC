using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.BuildingSystem.BuildingPart
{
    public class BuildingPart : MonoBehaviour, IBuildingPart
    {
        #region IBuildingPart
        public string ID { get => BuildingManager.Instance.BPartTableDictOnClass[this.classification.ToString()].id; set => throw new Exception("不允许设置建筑物的ID"); }

        [SerializeField, LabelText("分类")]
        private BuildingPartClassification classification;
        public BuildingPartClassification Classification { get => classification; private set => classification = value; }

        [SerializeField, LabelText("实例ID"), ReadOnly]
        private string instanceID;
        public string InstanceID { get => instanceID; set => instanceID = value; }

        GameObject IBuildingPart.gameObject { get => this.gameObject;  }
        Transform IBuildingPart.transform { get => this.transform;  }
        public bool isFirstBuild { get; protected set; }= true;
        [SerializeField, LabelText("能否放置"), ReadOnly]
        private bool canPlaceInPlaceMode = true;
        public bool CanPlaceInPlaceMode 
        {
            get
            {
                //Debug.Log($"{this.canPlaceInPlaceMode} {this.AttachedArea != null} {this.AttachedSocket != null} {this.CheckCanInPlaceMode.Invoke(this)}");\
                bool ans = (mode == BuildingMode.Edit || mode == BuildingMode.Place) && (this.GetComponent<Collider>() == null ? true : this.canPlaceInPlaceMode) && (this.AttachedArea != null || this.AttachedSocket != null) && (this.CheckCanInPlaceMode == null ? true : this.CheckCanInPlaceMode.Invoke(this));
                if (this.Mode == BuildingMode.Place || this.Mode == BuildingMode.Destroy || this.Mode == BuildingMode.Edit)
                {
                    if (this.Mode != BuildingMode.Destroy)
                    {
                        this.tmpTriggerMode = this.Mode;
                    }
                    this.Mode = ans ? this.tmpTriggerMode : BuildingMode.Destroy;
                }
                return ans;
            }
            set => canPlaceInPlaceMode = value; 
        }
        protected void CheckPladeMode()
        {
            bool ans = (this.GetComponent<Collider>() == null ? true : this.canPlaceInPlaceMode) && (this.AttachedArea != null || this.AttachedSocket != null) && (this.CheckCanInPlaceMode == null ? true : this.CheckCanInPlaceMode.Invoke(this));
            if (this.Mode == BuildingMode.Place || this.Mode == BuildingMode.Destroy || this.Mode == BuildingMode.Edit)
            {
                if (this.Mode != BuildingMode.Destroy)
                {
                    this.tmpTriggerMode = this.Mode;
                }
                this.Mode = ans ? this.tmpTriggerMode : BuildingMode.Destroy;
            }
        }

        public event IBuildingPart.CheckMode CheckCanInPlaceMode;
        public event IBuildingPart.CheckMode CheckCanEdit;
        public event IBuildingPart.CheckMode CheckCanDestory;

        public virtual void OnChangePlaceEvent(Vector3 oldPos, Vector3 newPos)
        {
            if (isFirstBuild)
            {
                isFirstBuild = false;
                BuildingManager.Instance.AddBuildingInstance(this);
            }
                
        }

        public bool CanEnterEditMode()
        {
            return CheckCanEdit == null || (CheckCanEdit != null && CheckCanEdit.Invoke(this));
        }

        public bool CanEnterDestoryMode()
        {
            return CheckCanDestory == null || (CheckCanDestory != null && CheckCanDestory.Invoke(this));
        }

        [SerializeField, LabelText("模式"), ReadOnly]
        private BuildingMode mode;
        public BuildingMode Mode
        {
            get => mode;
            set
            {
                mode = value;
                this.SetColliderTrigger((mode == BuildingMode.Place || mode == BuildingMode.Destroy || mode == BuildingMode.Edit));
                //if (mode == BuildingMode.Edit || mode == BuildingMode.Place) 
                //{
                //    this.CanPlaceInPlaceMode = true;
                //}
                //else
                //{
                //    this.CanPlaceInPlaceMode = false;
                //}
                
                Material mat = BuildingManager.Instance.GetBuldingMat(mode);
                if(mat != null)
                {
                    foreach (var kv in this.rowMat)
                    {
                        var mats = new List<Material>(kv.Value);
                        mats.Add(mat);
                        kv.Key.SetSharedMaterials(mats);
                        //kv.Key.sharedMaterial = mat;
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
        [ShowInInspector, ReadOnly]
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
                CheckPladeMode();
            }
        }
        private BuildingSocket.BuildingSocket attachedSocket;
        [ShowInInspector, ReadOnly]
        public BuildingSocket.BuildingSocket AttachedSocket
        {
            get => this.attachedSocket;
            set
            {
                if(value == null)
                {
                    this.ActiveSocket.AsMatchRotOffset = Quaternion.identity;
                    this.attachedSocket = null;
                }
                else if(value != null)
                {
                    if (this.ActiveSocket.CheckMatch(value))
                    {
                        if(this.attachedSocket != value)
                        {
                            this.ActiveSocket.AsMatchRotOffset = Quaternion.identity;
                        }
                        this.attachedSocket = value;
                    }
                    else
                    {

                        this.ActiveSocket.AsMatchRotOffset = Quaternion.identity;
                        this.attachedSocket = null;
                    }
                }
                CheckPladeMode();
            }
        }
        
        [SerializeField, ReadOnly]
        private int activeSocketIndex;
        [ShowInInspector, ReadOnly]
        public BuildingSocket.BuildingSocket ActiveSocket { 
            get
            {
                if(this.OwnedSocketList == null || activeSocketIndex < 0 || this.OwnedSocketList.Count <= activeSocketIndex)
                {
                    return null;
                }
                return this.OwnedSocketList[activeSocketIndex];
            }
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
            mat.ChildrenMat = new Dictionary<int, Material[]>();
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
                if(this.GetComponent<Renderer>())
                    this.GetComponent<Renderer>().sharedMaterials = mat.ParentMat;
            }
            if(mat.ChildrenMat != null)
            {
                foreach (var kv in mat.ChildrenMat)
                {
                    if(this.transform.GetChild(kv.Key).GetComponent<Renderer>())
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
        /// 原生材质
        /// </summary>
        [ShowInInspector]
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

            // 指向第一个可切换的Socket
            this.activeSocketIndex = 0;
            while (!this.ActiveSocket.IsActiveSocket)
            {
                this.activeSocketIndex = (this.activeSocketIndex + 1) % this.OwnedSocketList.Count;
            }
            this.ActiveSocket.OnActive();

            CheckPladeMode();


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
                        // 赋值顺序不能颠倒
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

            if (isTrigger == true)
            {
                if(this.GetComponent<Rigidbody>() == null)
                {
                    this.gameObject.AddComponent<Rigidbody>().isKinematic = true;
                }
                else
                {
                    this.gameObject.GetComponent<Rigidbody>().isKinematic = true;
                }
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
        [Button("根据资产名更正类型"), PropertyOrder(-1), ShowIf("IsPrefab")]
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
            //排除建造模式时手上拿的那一个
            if (!isFirstBuild)
            {
                BuildingManager.Instance.RemoveBuildingInstance(this);
            }
        }

        public virtual void OnEnterEdit()
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
