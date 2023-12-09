using ML.Engine.InventorySystem.CompositeSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.BuildingSystem.BuildingPart
{
    /// <summary>
    /// ������ӿ�
    /// </summary>
    public interface IBuildingPart
    {
        #region ID
        // to-do : PlaceMode���½�������ʱ�ḳ��InstanceID, ����һ�Σ�����浵��¼��ֱ���ý���������

        /// <summary>
        /// Prefab ����� => �����ʶ
        /// </summary>
        public BuildingPartClassification Classification { get; }

        /// <summary>
        /// ������ʵ��ID => ������ʵ��ID��� 2023-09-01-13-59-59-999 or ��ǰ����ʱ��
        /// </summary>
        public string InstanceID { get; set; }

        /// <summary>
        /// ������ GameObject
        /// </summary>
        public GameObject gameObject { get;  }

        /// <summary>
        /// ������ Transfrom
        /// </summary>
        public Transform transform { get; }
        #endregion

        #region Mode
        /// <summary>
        /// Trigger ��ײ���Ľ�� => �Ƿ��ܷ���
        /// </summary>
        public bool CanPlaceInPlaceMode { get; set; }

        public delegate bool CheckCanPlaceMode(IBuildingPart BPart);
        public event CheckCanPlaceMode CheckCanInPlaceMode;

        /// <summary>
        /// ��ǰ�����Ľ���ģʽ
        /// </summary>
        public BuildingMode Mode { get; set; }

        // to-do : Mode ����ʱ��һ�����Ĳ��ʱ���, ʵ�ֽӿ�ʱ�ھ������ڲ�����ʵ��
        ///// <summary>
        ///// �洢ԭ���Ĳ���
        ///// </summary>
        //Dictionary<Renderer, Material> RowMat { get; set; }
        #endregion

        #region ��ת
        /// <summary>
        /// �����ʼ��ת
        /// </summary>
        public Quaternion BaseRotation { get; }

        /// <summary>
        /// ��תƫ����
        /// ������ת = BaseRotation -> Socket|Area [ -> RotOffset]
        /// </summary>
        public Quaternion RotOffset { get; set; }
        #endregion

        #region BuildingArea
        /// <summary>
        /// ������Area,����ʱ����λ�ú���ת���������ȼ�����AttachedSocket
        /// </summary>
        public BuildingArea.BuildingArea AttachedArea { get; set; }
        /// <summary>
        /// ����BuildingAreaƥ���ActiveSocket.AreaType
        /// </summary>
        public BuildingSocket.BuildingSocketType ActiveAreaType
        {
            get
            {
                return this.ActiveSocket == null ? BuildingSocket.BuildingSocketType.None : this.ActiveSocket.CanPlaceAreaType;
            }
        }
        #endregion

        #region BuildingSocket
        /// <summary>
        /// ������Socket => ����ʱֱ�Ӹ���λ�ú���ת, ����Area
        /// </summary>
        public BuildingSocket.BuildingSocket AttachedSocket { get; set; }

        /// <summary>
        /// ��ǰ�Լ���������ƥ��Socket��Socket
        /// ֻ�д���Place״̬ʱ����Ч
        /// ����ʱ������Ϊnull
        /// </summary>
        public BuildingSocket.BuildingSocket ActiveSocket { get; set; }

        /// <summary>
        /// ӵ�е�Socket�б������ֻ�
        /// </summary>
        public List<BuildingSocket.BuildingSocket> OwnedSocketList { get; }

        /// <summary>
        /// ע��BuildingSocket
        /// </summary>
        /// <param name="socket"></param>
        public void AddBuildingSocket(BuildingSocket.BuildingSocket socket)
        {
            if(!this.OwnedSocketList.Contains(socket))
            {
                this.OwnedSocketList.Add(socket);
            }
        }

        /// <summary>
        /// ע��BuildingSocket
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public bool RemoveBuildingSocket(BuildingSocket.BuildingSocket socket)
        {
            return this.OwnedSocketList.Remove(socket);
        }

        /// <summary>
        /// �ֻ�����Ĳ��
        /// ���� ActiveSocket
        /// ����λ�ú���ת
        /// </summary>
        public abstract void AlternativeActiveSocket();
        #endregion

        #region Material
        /// <summary>
        /// ��õ�ǰMode��Ӧ�Ĳ���
        /// </summary>
        /// <returns></returns>
        public virtual Material GetCurModeMat()
        {
            return BuildingManager.Instance.GetBuldingMat(this.Mode);
        }

        /// <summary>
        /// ��ÿ����Ĳ���
        /// </summary>
        /// <returns></returns>
        public abstract BuildingCopiedMaterial GetCopiedMaterial();

        /// <summary>
        /// ճ�������Ĳ���
        /// </summary>
        /// <param name="copiedMaterial"></param>
        public abstract void SetCopiedMaterial(BuildingCopiedMaterial copiedMaterial);
        #endregion

        #region CheckTrigger
        /// <summary>
        /// ���һ�μ�⵽�Ĳ��ܷ��õ�֡�� FrameCount
        /// </summary>
        public int lastTriggerFrameCount { get; set; }
        public BuildingMode tmpTriggerMode { get; set; }
        public void CheckTriggerStay(Collider other)
        {
            if(this.Mode == BuildingMode.Place || this.Mode == BuildingMode.Destroy || this.Mode == BuildingMode.Edit)
            {
                if(this.Mode != BuildingMode.Destroy)
                {
                    this.tmpTriggerMode = this.Mode;
                }
                if (other.GetComponent<IBuildingPart>() != null)
                {
                    this.CanPlaceInPlaceMode = false;
                    this.Mode = BuildingMode.Destroy;
                    lastTriggerFrameCount = Time.frameCount;
                }
                // ͬһ֡��⵽���ܷ��ã����֡�Ͳ��ܸ����ܷ����ֵ����Ϊ���ܷ���
                else if(lastTriggerFrameCount != Time.frameCount)
                {
                    this.CanPlaceInPlaceMode = true;
                    this.Mode = this.tmpTriggerMode;
                }
                this.Mode = this.CanPlaceInPlaceMode ? this.tmpTriggerMode : BuildingMode.Destroy;
            }
        }
        #endregion

    }
}

