using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ML.Engine.SaveSystem
{
    /// <summary>
    /// 存档设置
    /// </summary>
    public class SaveConfig
    {
        /// <summary>
        /// 存档文件类型
        /// </summary>
        public SaveType SaveType;
        /// <summary>
        /// 是否加密
        /// </summary>
        public bool UseEncrption;
        public SaveConfig()
        {
            this.SaveType = SaveType.Json;
            this.UseEncrption = false;
        }
        public SaveConfig(SaveType SaveType, bool UseEncrption)
        {
            this.SaveType = SaveType;
            this.UseEncrption = UseEncrption;
        }
        public SaveConfig(SaveConfig config)
        {
            if (config != null)
            {
                this.SaveType = config.SaveType;
                this.UseEncrption = config.UseEncrption;
            }
        }
        public void Init(SaveConfig config)
        {
            if (config != null)
            {
                this.SaveType = config.SaveType;
                this.UseEncrption = config.UseEncrption;
            }
        }
    }
}