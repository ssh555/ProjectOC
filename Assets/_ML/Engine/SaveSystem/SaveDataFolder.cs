using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ML.Engine.SaveSystem
{
    /// <summary>
    /// ��Ӧһ���浵�ļ����µ�SaveConfig�ļ�
    /// ��˴浵���������(����ʵ������)
    /// �浵·��Path + �浵����name == �浵�����ļ���
    /// �ڴ��ļ�������һ��SaveConfig�ļ������Ǵ洢��SaveDataFolder����
    /// �����ļ�Ϊ�浵�����ļ�
    /// </summary>
    public class SaveDataFolder : ISaveData
    {
        /// <summary>
        /// �˴浵������
        /// </summary>
        public string Name;

        /// <summary>
        /// �浵����ʱ��
        /// </summary>
        public string CreateTime;

        /// <summary>
        /// ����޸�ʱ��
        /// </summary>
        public string LastSaveTime;

        /// <summary>
        /// �浵�ļ�����-��Ӧ�����ݽṹ����ȫ���ַ�����ӳ���
        /// SaveName -> Path+Name+SaveName+(.bytes|.json)Ϊʵ�ʵĴ洢����·��
        /// </summary>
        public Dictionary<string, string> FileMap = new Dictionary<string, string>();
    }
}