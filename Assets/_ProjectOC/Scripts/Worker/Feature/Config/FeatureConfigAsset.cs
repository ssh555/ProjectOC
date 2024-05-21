using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.WorkerNS
{
    [CreateAssetMenu(fileName = "FeatureConfigAsset", menuName = "OC/Worker/FeatureConfigAsset", order = 1)]
    public class FeatureConfigAsset : SerializedScriptableObject
    {
        [LabelText("��������"), ShowInInspector]
        public FeatureConfig Config;
    }
}