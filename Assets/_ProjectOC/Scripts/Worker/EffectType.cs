using Sirenix.OdinInspector;
namespace ProjectOC.WorkerNS
{
    /// <summary>
    /// 刁民效果类型
    /// </summary>
    public enum EffectType
    {
        [LabelText("None")]
        None,

        #region Set Int
        [LabelText("变更体力上限")]
        AlterAPMax,

        [LabelText("烹饪经验速度")]
        AlterExpRate_Cook,
        [LabelText("轻工经验速度")]
        AlterExpRate_HandCraft,
        [LabelText("精工经验速度")]
        AlterExpRate_Industry,
        [LabelText("术法经验速度")]
        AlterExpRate_Magic,
        [LabelText("搬运经验速度")]
        AlterExpRate_Transport,
        [LabelText("采集经验速度")]
        AlterExpRate_Collect,
        #endregion

        #region Offset Int
        [LabelText("变更负重上限")]
        AlterBURMax,

        [LabelText("变更烹饪效率加成")]
        AlterEff_Cook,
        [LabelText("变更轻工效率加成")]
        AlterEff_HandCraft,
        [LabelText("变更精工效率加成")]
        AlterEff_Industry,
        [LabelText("变更术法效率加成")]
        AlterEff_Magic,
        [LabelText("变更搬运效率加成")]
        AlterEff_Transport,
        [LabelText("变更采集效率加成")]
        AlterEff_Collect,
        #endregion

        #region Offset Float
        [LabelText("变更移动速度")]
        AlterWalkSpeed,
        #endregion
    }
}