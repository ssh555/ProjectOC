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
        /// <summary>
        /// ������Կ 256λ
        /// </summary>
        private byte[] Key = new byte[] {0x2B, 0x7E, 0x15, 0x16, 0x28, 0xAE, 0xD2, 0xA6};
        /// <summary>
        /// ������ɵĳ�ʼ������
        /// </summary>
        private byte[] IV = new byte[] {0x6A, 0x8F, 0xE2, 0x4B, 0x7D, 0x1A, 0x5C, 0x3E};

        private DESCryptoServiceProvider CryptoProvider = new DESCryptoServiceProvider();

        /// <summary>
        /// ������
        /// </summary>
        /// <param name="data">����</param>
        /// <param name="useEncryption">�Ƿ����</param>
        public abstract void SaveData<T>(T data, bool useEncryption) where T : ISaveData;

        /// <summary>
        /// �������ݣ�·��Ϊ������׺�����·��
        /// </summary>
        /// <param name="relativePathWithoutSuffix">Application.persistent·������׺Ϊ.bytes����.json���Զ����</param>
        /// <param name="useEncryption">�Ƿ����</param>
        /// <returns></returns>
        public abstract T LoadData<T>(string relativePathWithoutSuffix, bool useEncryption) where T : ISaveData;

        /// <summary>
        /// ��ȡ�ڴ�������
        /// </summary>
        /// <param name="memory">�ڴ���</param>
        /// <param name="useEncryption">�Ƿ����</param>
        public abstract T LoadData<T>(Stream memory, bool useEncryption) where T : ISaveData;

        /// <summary>
        /// ����������
        /// </summary>
        protected Stream EncryptorStream(Stream stream)
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
        protected Stream DecryptorStream(Stream stream)
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
