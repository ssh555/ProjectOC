using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace ML.Engine.SaveSystem
{
    /// <summary>
    /// �浵�������ͽӿ�
    /// </summary>
    public class ISaveData : ICloneable
    {
        /// <summary>
        /// ������ļ���
        /// </summary>
        [NonSerialized, IgnoreDataMember]
        public string SaveName;
        /// <summary>
        /// �洢�����·��λ�ã��������ļ���
        /// </summary>
        [NonSerialized, IgnoreDataMember]
        public string Path;
        /// <summary>
        /// �Ƿ������ݸ��£����Ƿ���Ҫ���´浵
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