using Sirenix.OdinInspector;

namespace ProjectOC.WorkerNS
{
    [System.Serializable]
    public enum WorkerCategory
    {
        None,
        [LabelText("���")]
        Random,
        [LabelText("�����")]
        CookWorker,
        [LabelText("�ֹ���")]
        HandCraftWorker,
        [LabelText("������")]
        IndustryWorker,
        [LabelText("������")]
        MagicWorker,
        [LabelText("������")]
        TransportWorker,
        [LabelText("�ɼ���")]
        CollectWorker
    }
}