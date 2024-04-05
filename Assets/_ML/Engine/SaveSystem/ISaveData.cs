using System.Runtime.Serialization;

namespace ML.Engine.SaveSystem
{
    /// <summary>
    /// 存档数据类型接口
    /// </summary>
    public interface ISaveData
    {
        /// <summary>
        /// 存储的相对路径位置，不包括文件名
        /// </summary>
        [IgnoreDataMember]
        public string SavePath { get; set; }
        /// <summary>
        /// 保存的文件名
        /// </summary>
        [IgnoreDataMember]
        public string SaveName { get; set; }
        /// <summary>
        /// 是否有数据更新，即是否需要更新存档
        /// </summary>
        [IgnoreDataMember]
        public bool IsDirty { get; set; }
        /// <summary>
        /// 版本号
        /// </summary>
        [DataMember]
        public ML.Engine.Utility.Version Version { get; set; }
        public object Clone();
    }
}