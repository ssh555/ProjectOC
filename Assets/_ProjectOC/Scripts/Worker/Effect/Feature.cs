using ProjectOC.ManagerNS;
using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace ProjectOC.WorkerNS
{
    [LabelText("����"), System.Serializable]
    public class Feature
    {
        public string ID = "";
        public List<Effect> Effects = new List<Effect>();

        #region ��������
        [LabelText("�����"), ShowInInspector, ReadOnly]
        public string IDExclude { get => LocalGameManager.Instance != null ? LocalGameManager.Instance.FeatureManager.GetIDExclude(ID) : ""; }
        /// <summary>
        /// ��ţ��������򣬴��ϵ��µ�˳��Ϊ���塢���桢���桢��������
        /// </summary>
        [LabelText("���"), ShowInInspector, ReadOnly]
        public int Sort { get => LocalGameManager.Instance != null ? LocalGameManager.Instance.FeatureManager.GetSort(ID) : 10; }
        [LabelText("����"), ShowInInspector, ReadOnly]
        public string Name { get => LocalGameManager.Instance != null ? LocalGameManager.Instance.FeatureManager.GetName(ID) : ""; }
        [LabelText("����"), ShowInInspector, ReadOnly]
        public FeatureType FeatureType { get => LocalGameManager.Instance != null ? LocalGameManager.Instance.FeatureManager.GetFeatureType(ID) : FeatureType.None; }
        [LabelText("�����������ı�"), ShowInInspector, ReadOnly]
        public string Description { get => LocalGameManager.Instance != null ? LocalGameManager.Instance.FeatureManager.GetItemDescription(ID) : ""; }
        [LabelText("����Ч���������ı�"), ShowInInspector, ReadOnly]
        public string EffectsDescription { get => LocalGameManager.Instance != null ? LocalGameManager.Instance.FeatureManager.GetEffectsDescription(ID) : ""; }
        #endregion

        public Feature(FeatureTableData config)
        {
            ID = config.ID;
            Effects = new List<Effect>();
            foreach (var tuple in config.Effects)
            {
                Effects.Add(LocalGameManager.Instance.EffectManager.SpawnEffect(tuple.Item1, tuple.Item2));
            }
        }

        public void ApplyFeature(Worker worker)
        {
            if (worker != null)
            {
                foreach (Effect effect in Effects)
                {
                    worker.ApplyEffect(effect);
                }
            }
        }

        public class FeatureSort : IComparer<Feature>
        {
            public int Compare(Feature x, Feature y)
            {
                if (x == null)
                {
                    if (y == null)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }
                if (y == null)
                {
                    return -1;
                }
                int xs = x.Sort;
                int ys = y.Sort;
                if (xs != ys)
                {
                    return xs.CompareTo(ys);
                }
                else
                {
                    return string.Compare(x.ID, y.ID);
                }
            }
        }
    }
}

