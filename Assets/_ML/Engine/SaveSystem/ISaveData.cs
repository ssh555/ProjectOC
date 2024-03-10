using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace ML.Engine.SaveSystem
{
    /// <summary>
    /// 存档数据类型接口
    /// </summary>
    public class ISaveData
    {
        /// <summary>
        /// 保存的文件名
        /// </summary>
        [NonSerialized, IgnoreDataMember]
        public string SaveName;
        /// <summary>
        /// 是否有数据更新，即是否需要更新存档
        /// </summary>
        [NonSerialized, IgnoreDataMember]
        public bool IsDirty;
        /// <summary>
        /// 存储的相对路径位置
        /// </summary>
        [NonSerialized, IgnoreDataMember]
        public string Path;
    }
}