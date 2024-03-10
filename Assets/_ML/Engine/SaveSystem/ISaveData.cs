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
    public class ISaveData
    {
        /// <summary>
        /// ������ļ���
        /// </summary>
        [NonSerialized, IgnoreDataMember]
        public string SaveName;
        /// <summary>
        /// �Ƿ������ݸ��£����Ƿ���Ҫ���´浵
        /// </summary>
        [NonSerialized, IgnoreDataMember]
        public bool IsDirty;
        /// <summary>
        /// �洢�����·��λ��
        /// </summary>
        [NonSerialized, IgnoreDataMember]
        public string Path;
    }
}