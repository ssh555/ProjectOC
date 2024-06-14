using Sirenix.OdinInspector;

[LabelText("性别")]
public enum Gender
{
    [LabelText("无")]
    None,
    [LabelText("男性")]
    Male,
    [LabelText("女性")]
    Female,
    [LabelText("二形")]
    Futanari,
    [LabelText("中性")]
    Neutral,
    [LabelText("不明")]
    Unknown
}

[LabelText("性取向")]
public enum SexPreference
{
    None,
    [LabelText("异性")]
    Hetero,
    [LabelText("同性")]
    Homo,
    [LabelText("双性")]
    Bisex,
    [LabelText("无性")]
    Asex,
    [LabelText("不明")]
    Unknown
}
