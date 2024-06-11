using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.ClanNS
{
    [CreateAssetMenu(fileName = "ClanConfigAsset", menuName = "OC/Clan/ClanConfigAsset", order = 1)]
    public class ClanConfigAsset : SerializedScriptableObject
    {
        [LabelText("≈‰÷√ ˝æ›"), ShowInInspector]
        public ClanConfig Config;
    }
}