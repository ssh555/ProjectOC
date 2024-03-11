using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ML.Engine.SaveSystem
{
    /// <summary>
    /// 存档设置
    /// </summary>
    public struct SaveConfig
    {
        /// <summary>
        /// 存档文件类型
        /// </summary>
        public SaveType SaveType;
        /// <summary>
        /// 是否加密
        /// </summary>
        public bool UseEncrption;
        public SaveConfig(SaveType SaveType = SaveType.Json, bool UseEncrption = false)
        {
            this.SaveType = SaveType;
            this.UseEncrption = UseEncrption;
        }
        public SaveConfig(SaveConfig config)
        {
            this.SaveType = config.SaveType;
            this.UseEncrption = config.UseEncrption;
        }
    }
}