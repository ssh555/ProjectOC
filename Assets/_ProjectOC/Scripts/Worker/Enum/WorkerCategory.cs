using Sirenix.OdinInspector;

namespace ProjectOC.WorkerNS
{
    [System.Serializable]
    public enum WorkerCategory
    {
        None,
        [LabelText("随机")]
        Random,
        [LabelText("烹饪兽")]
        CookWorker,
        [LabelText("手工兽")]
        HandCraftWorker,
        [LabelText("精工兽")]
        IndustryWorker,
        [LabelText("术法兽")]
        MagicWorker,
        [LabelText("搬运兽")]
        TransportWorker,
        [LabelText("采集兽")]
        CollectWorker
    }
}