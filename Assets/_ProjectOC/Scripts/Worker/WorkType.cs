using Sirenix.OdinInspector;

namespace ProjectOC.WorkerNS
{
    [LabelText("职业类型")]
    public enum WorkType
    {
        [LabelText("None")]
        None,
        [LabelText("烹饪")]
        Cook,
        [LabelText("轻工")]
        HandCraft,
        [LabelText("精工")]
        Industry,
        [LabelText("术法")]
        Magic,
        [LabelText("搬运")]
        Transport,
        [LabelText("采集")]
        Collect
    }
}
