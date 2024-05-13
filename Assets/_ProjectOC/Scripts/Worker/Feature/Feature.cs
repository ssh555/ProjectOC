using ProjectOC.ManagerNS;
using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace ProjectOC.WorkerNS
{
    [LabelText("����"), System.Serializable]
    public struct Feature : IComparer<Feature>
    {
        public string ID;
        private bool isConditionTrue;
        [ShowInInspector]
        public bool IsConditionTrue
        {
            get => isConditionTrue;
            set
            {
                if (isConditionTrue != value)
                {
                    RemoveFeature();
                }
                isConditionTrue = value;
                ApplyFeature();
            }
        }
        public Worker Owner;

        #region Property
        [LabelText("���"), ShowInInspector, ReadOnly]
        public int Sort => LocalGameManager.Instance != null ? LocalGameManager.Instance.FeatureManager.GetSort(ID) : int.MaxValue;
        [LabelText("����"), ShowInInspector, ReadOnly]
        public string Name => LocalGameManager.Instance != null ? LocalGameManager.Instance.FeatureManager.GetName(ID) : "";
        [LabelText("ͼ��"), ShowInInspector, ReadOnly]
        public string Icon => LocalGameManager.Instance != null ? LocalGameManager.Instance.FeatureManager.GetIcon(ID) : "";
        [LabelText("����"), ShowInInspector, ReadOnly]
        public FeatureType Type => LocalGameManager.Instance != null ? LocalGameManager.Instance.FeatureManager.GetFeatureType(ID) : FeatureType.None;
        [LabelText("������"), ShowInInspector, ReadOnly]
        public string UpgradeID => LocalGameManager.Instance != null ? LocalGameManager.Instance.FeatureManager.GetUpgradeID(ID) : "";
        [LabelText("������"), ShowInInspector, ReadOnly]
        public string ReduceID => LocalGameManager.Instance != null ? LocalGameManager.Instance.FeatureManager.GetReduceID(ID) : "";
        [LabelText("��ת��"), ShowInInspector, ReadOnly]
        public string ReverseID => LocalGameManager.Instance != null ? LocalGameManager.Instance.FeatureManager.GetReverseID(ID) : "";
        [LabelText("����"), ShowInInspector, ReadOnly]
        public string Condition => LocalGameManager.Instance != null ? LocalGameManager.Instance.FeatureManager.GetCondition(ID) : "";
        [LabelText("�¼�����"), ShowInInspector, ReadOnly]
        public string Event => LocalGameManager.Instance != null ? LocalGameManager.Instance.FeatureManager.GetEvent(ID) : "";
        [LabelText("��������"), ShowInInspector, ReadOnly]
        public string Description => LocalGameManager.Instance != null ? LocalGameManager.Instance.FeatureManager.GetItemDescription(ID) : "";
        [LabelText("Ч������"), ShowInInspector, ReadOnly]
        public string EffectsDescription => LocalGameManager.Instance != null ? LocalGameManager.Instance.FeatureManager.GetEffectsDescription(ID) : "";
        #endregion

        public Feature(FeatureTableData config)
        {
            ID = config.ID;
            isConditionTrue = true;
            Owner = null;
        }

        public void SetOwner(Worker worker)
        {
            Owner = worker;
            ApplyFeature();
            if (!string.IsNullOrEmpty(Event))
            {
                ML.Engine.Manager.GameManager.Instance.EventManager.ExecuteEvent(Event);
            }
        }

        public void ClearOwner()
        {
            RemoveFeature();
            Owner = null;
            if (!string.IsNullOrEmpty(Event))
            {
                ML.Engine.Manager.GameManager.Instance.EventManager.ExecuteEvent("Remove" + Event);
            }
        }

        private void ApplyFeature()
        {
            if (Owner != null)
            {
                var effects = IsConditionTrue ? LocalGameManager.Instance.FeatureManager.GetTrueEffect(ID) : 
                    LocalGameManager.Instance.FeatureManager.GetFalseEffect(ID);
                foreach (var effect in effects)
                {
                    Owner.ApplyEffect(LocalGameManager.Instance.EffectManager.SpawnEffect(effect));
                }
            }
        }

        private void RemoveFeature()
        {
            if (Owner != null)
            {
                var effects = IsConditionTrue ? LocalGameManager.Instance.FeatureManager.GetTrueEffect(ID) :
                    LocalGameManager.Instance.FeatureManager.GetFalseEffect(ID);
                foreach (var effect in effects)
                {
                    Owner.RemoveEffect(LocalGameManager.Instance.EffectManager.SpawnEffect(effect));
                }
            }
        }

        public int Compare(Feature x, Feature y)
        {
            int xt = (int)x.Type;
            int yt = (int)y.Type;
            if (xt != yt)
            {
                return yt.CompareTo(xt);
            }
            int xs = x.Sort;
            int ys = y.Sort;
            if (xs != ys)
            {
                return xs.CompareTo(ys);
            }
            return string.Compare(x.ID, y.ID);
        }
    }
}