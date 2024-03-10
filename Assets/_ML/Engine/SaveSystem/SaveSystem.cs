using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace ML.Engine.SaveSystem
{
    /// <summary>
    /// 只负责处理逻辑，处理文件的读取
    /// </summary>
    public abstract class SaveSystem
    {
        /// <summary>
        /// 保存文件的路径
        /// </summary>
        protected string SavePath = Path.Combine(Application.persistentDataPath, "Save");
        // 加密密钥和初始化向量（IV）
        protected byte[] Key = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };
        protected byte[] IV = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };

        protected DESCryptoServiceProvider CryptoProvider = new DESCryptoServiceProvider();

        /// <summary>
        /// 存数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="useEncryption">是否加密</param>
        public abstract void SaveData(ISaveData data, bool useEncryption);

        /// <summary>
        /// 加载数据，路径为不带后缀的相对路径
        /// </summary>
        /// <param name="relativePathWithoutSuffix">Application.persistent路径，后缀为.bytes或者.json，自动添加</param>
        /// <param name="useEncryption">是否加密</param>
        /// <returns></returns>
        public abstract ISaveData LoadData(string relativePathWithoutSuffix, bool useEncryption);

        /// <summary>
        /// 读取内存流数据
        /// </summary>
        /// <param name="memory">内存流</param>
        /// <param name="useEncryption">是否加密</param>
        public abstract ISaveData LoadData(Stream memory, bool useEncryption);

        /// <summary>
        /// 加密数据流
        /// </summary>
        public Stream EncryptorStream(Stream stream)
        {
            // 使用提供的加密密钥和初始化向量
            CryptoProvider.Key = Key;
            CryptoProvider.IV = IV;

            // 创建 CryptoStream 来加密数据
            CryptoStream cryptoStream = new CryptoStream(stream, CryptoProvider.CreateEncryptor(), CryptoStreamMode.Write);
            return cryptoStream;
        }

        /// <summary>
        /// 解密数据流
        /// </summary>
        public Stream DecryptorStream(Stream stream)
        {
            // 使用提供的加密密钥和初始化向量
            CryptoProvider.Key = Key;
            CryptoProvider.IV = IV;

            // 创建 CryptoStream 来解密数据
            CryptoStream cryptoStream = new CryptoStream(stream, CryptoProvider.CreateDecryptor(), CryptoStreamMode.Read);
            return cryptoStream;
        }
    }
}