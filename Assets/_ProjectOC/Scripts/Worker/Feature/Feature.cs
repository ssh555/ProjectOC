using ProjectOC.ManagerNS;
using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace ProjectOC.WorkerNS
{
    [LabelText("特性"), System.Serializable]
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
        [LabelText("序号"), ShowInInspector, ReadOnly]
        public int Sort => LocalGameManager.Instance != null ? LocalGameManager.Instance.FeatureManager.GetSort(ID) : int.MaxValue;
        [LabelText("类型"), ShowInInspector, ReadOnly]
        public FeatureType Type => LocalGameManager.Instance != null ? LocalGameManager.Instance.FeatureManager.GetFeatureType(ID) : FeatureType.None;
        [LabelText("事件函数"), ShowInInspector, ReadOnly]
        public string Event => LocalGameManager.Instance != null ? LocalGameManager.Instance.FeatureManager.GetEvent(ID) : "";
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
                ML.Engine.Manager.GameManager.Instance.EventManager.ExecuteEvent(Event, Owner);
            }
            Owner.Feature.Add(ID, this);
        }

        public void ClearOwner()
        {
            RemoveFeature();
            if (!string.IsNullOrEmpty(Event))
            {
                ML.Engine.Manager.GameManager.Instance.EventManager.ExecuteEvent("Remove" + Event, Owner);
            }
            Owner.Feature.Remove(ID);
            Owner = null;
        }

        private void ApplyFeature()
        {
            if (Owner != null)
            {
                var effects = isConditionTrue ? LocalGameManager.Instance.FeatureManager.GetTrueEffect(ID) : 
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
                var effects = isConditionTrue ? LocalGameManager.Instance.FeatureManager.GetTrueEffect(ID) :
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