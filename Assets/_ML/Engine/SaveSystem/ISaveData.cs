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
    public class ISaveData : ICloneable
    {
        /// <summary>
        /// 保存的文件名
        /// </summary>
        [NonSerialized, IgnoreDataMember]
        public string SaveName;
        /// <summary>
        /// 存储的相对路径位置，不包括文件名
        /// </summary>
        [NonSerialized, IgnoreDataMember]
        public string Path;
        /// <summary>
        /// 是否有数据更新，即是否需要更新存档
        /// </summary>
        [NonSerialized, IgnoreDataMember]
        public bool IsDirty;
        public ISaveData()
        {
            SaveName = "";
            Path = "";
            IsDirty = false;
        }

        public ISaveData(string path, string name)
        {
            SaveName = name;
            Path = path;
            IsDirty = false;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}