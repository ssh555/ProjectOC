using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace ML.Engine.SaveSystem
{
    /// <summary>
    /// ֻ�������߼��������ļ��Ķ�ȡ
    /// </summary>
    public abstract class SaveSystem
    {
        /// <summary>
        /// �����ļ���·��
        /// </summary>
        protected string SavePath = Path.Combine(Application.persistentDataPath, "Save");
        // ������Կ�ͳ�ʼ��������IV��
        protected byte[] Key = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };
        protected byte[] IV = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };

        protected DESCryptoServiceProvider CryptoProvider = new DESCryptoServiceProvider();

        /// <summary>
        /// ������
        /// </summary>
        /// <param name="data">����</param>
        /// <param name="useEncryption">�Ƿ����</param>
        public abstract void SaveData(ISaveData data, bool useEncryption);

        /// <summary>
        /// �������ݣ�·��Ϊ������׺�����·��
        /// </summary>
        /// <param name="relativePathWithoutSuffix">Application.persistent·������׺Ϊ.bytes����.json���Զ����</param>
        /// <param name="useEncryption">�Ƿ����</param>
        /// <returns></returns>
        public abstract ISaveData LoadData(string relativePathWithoutSuffix, bool useEncryption);

        /// <summary>
        /// ��ȡ�ڴ�������
        /// </summary>
        /// <param name="memory">�ڴ���</param>
        /// <param name="useEncryption">�Ƿ����</param>
        public abstract ISaveData LoadData(Stream memory, bool useEncryption);

        /// <summary>
        /// ����������
        /// </summary>
        public Stream EncryptorStream(Stream stream)
        {
            // ʹ���ṩ�ļ�����Կ�ͳ�ʼ������
            CryptoProvider.Key = Key;
            CryptoProvider.IV = IV;

            // ���� CryptoStream ����������
            CryptoStream cryptoStream = new CryptoStream(stream, CryptoProvider.CreateEncryptor(), CryptoStreamMode.Write);
            return cryptoStream;
        }

        /// <summary>
        /// ����������
        /// </summary>
        public Stream DecryptorStream(Stream stream)
        {
            // ʹ���ṩ�ļ�����Կ�ͳ�ʼ������
            CryptoProvider.Key = Key;
            CryptoProvider.IV = IV;

            // ���� CryptoStream ����������
            CryptoStream cryptoStream = new CryptoStream(stream, CryptoProvider.CreateDecryptor(), CryptoStreamMode.Read);
            return cryptoStream;
        }
    }
}