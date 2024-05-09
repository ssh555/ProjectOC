using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.WorkerNS
{
    [CreateAssetMenu(fileName = "WorkerConfigAsset", menuName = "OC/Worker/WorkerConfigAsset", order = 1)]
    public class WorkerConfigAsset : SerializedScriptableObject
    {
        [LabelText("≈‰÷√ ˝æ›")]
        public WorkerConfig Config;
    }
}