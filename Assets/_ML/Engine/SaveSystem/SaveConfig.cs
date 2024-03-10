using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ML.Engine.SaveSystem
{
    /// <summary>
    /// �浵����
    /// </summary>
    public class SaveConfig
    {
        /// <summary>
        /// �浵�ļ�����
        /// </summary>
        public SaveType SaveType;
        /// <summary>
        /// �Ƿ����
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