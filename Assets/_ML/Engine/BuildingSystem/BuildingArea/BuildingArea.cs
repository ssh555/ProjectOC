using ML.Engine.BuildingSystem.BuildingPart;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.BuildingSystem.BuildingArea
{
    // Require Collider
    // ��ײ��⽻��BuildingSocket�Լ����
    [RequireComponent(typeof(Collider))]
    public class BuildingArea : MonoBehaviour
    {
        #region Static Grid
        /// <summary>
        /// ���������Ļ�����λ���ӵı߳�����λΪm
        /// </summary>
        [LabelText("������λ���ӱ߳�"), PropertyTooltip("��λ m"), Range(0, 100), FoldoutGroup("����"), ShowInInspector]
        public static float BaseGridSideLength = 0.50f;

        /// <summary>
        /// ���ڰ���������λ���ӵı߳�����λΪm
        /// </summary>
        [LabelText("������λ���ӱ߳�"), PropertyTooltip("��λ m"), Range(0, 1000), FoldoutGroup("����"), ShowInInspector]
        public static float BoundGridSideLength = 10.00f;
        #endregion

        /// <summary>
        /// ����BPartʱ�Ƿ�������ת
        /// </summary>
        [LabelText("����BPart��תƫ��"), PropertyTooltip("����BPartʱ�Ƿ���������תƫ��"), FoldoutGroup("��������"), SerializeField]
        public bool IsCanRotate = false;

        /// <summary>
        /// ������ײ���Collider
        /// </summary>
        protected new Collider collider;

        /// <summary>
        /// ��ȡ��Area��ƥ��������
        /// ���ظ������ֵΪ��Чλ��ֵ
        /// ����NanΪ��Ч��תֵ
        /// </summary>
        /// <returns></returns>
        public bool GetMatchPointOnArea(Vector3 point, float radius, out Vector3 pos, out Quaternion rot)
        {
            // λ�� Bound ��
            if(this.collider.bounds.Contains(point))
            {
                Vector3 scale = this.transform.localScale;
                this.transform.localScale = Vector3.one;

                // point �� Area ����ϵ�µı�ʾ
                Vector3 localPoint = this.transform.InverseTransformPoint(point);

                // ��������ֲ�����
                Vector3 localNearestGP = new Vector3(Mathf.RoundToInt(localPoint.x / BaseGridSideLength), localPoint.y, Mathf.RoundToInt(localPoint.z / BaseGridSideLength)) * BaseGridSideLength;

                // ������������ && ���ڷ�Χ�� ? ����ԭ�� : ���������
                pos =(!BuildingManager.Instance.Placer.IsEnableGridSupport && Vector3.Distance(localPoint, localNearestGP) > radius) ? point : this.transform.TransformPoint(localNearestGP);
                // ʹ��Area.Rotation
                rot = this.transform.rotation;
                this.transform.localScale = scale;
                return true;
            }

            // ������ Area ��
            pos = Vector3.negativeInfinity;
            rot = new Quaternion(float.NaN, float.NaN, float.NaN, float.NaN);
            return false;
        }

        /// <summary>
        /// ����ƥ���AreaType
        /// </summary>
        [LabelText("ƥ������")]
        public BuildingSocket.BuildingSocketType Type;

        /// <summary>
        /// Ŀ��Part�Ƿ��ܷ����ڴ�����
        /// </summary>
        /// <param name="BPart"></param>
        /// <returns></returns>
        public bool CheckAreaTypeMatch(IBuildingPart BPart)
        {
            return (BPart.ActiveAreaType & this.Type) != 0;
        }
        
        /// <summary>
        /// ����������ʱ��Area�����������
        /// </summary>
        public virtual void DrawGrid()
        {
            // to-do
        }

        private void Awake()
        {
            this.collider = this.GetComponent<Collider>();
            this.collider.isTrigger = true;
        }

        private void Start()
        {
            StartCoroutine(__DelayInit__());
        }

        private IEnumerator __DelayInit__()
        {
            while(BuildingManager.Instance == null || BuildingManager.Instance.BuildingAreaList == null)
            {
                yield return null;
            }
            BuildingManager.Instance.BuildingAreaList.Add(this);
        }

        private void OnEnable()
        {
            this.collider.enabled = true;
        }

        private void OnDisable()
        {
            this.collider.enabled = false;
        }

        private void OnDestroy()
        {
            BuildingManager.Instance.BuildingAreaList.Remove(this);
        }
    }
}

