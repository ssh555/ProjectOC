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
        /// <summary>
        /// 加密密钥 256位
        /// </summary>
        private byte[] Key = new byte[] {0x2B, 0x7E, 0x15, 0x16, 0x28, 0xAE, 0xD2, 0xA6};
        /// <summary>
        /// 随机生成的初始化向量
        /// </summary>
        private byte[] IV = new byte[] {0x6A, 0x8F, 0xE2, 0x4B, 0x7D, 0x1A, 0x5C, 0x3E};

        private DESCryptoServiceProvider CryptoProvider = new DESCryptoServiceProvider();

        /// <summary>
        /// 存数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="useEncryption">是否加密</param>
        public abstract void SaveData<T>(T data, bool useEncryption) where T : ISaveData;

        /// <summary>
        /// 加载数据，路径为不带后缀的相对路径
        /// </summary>
        /// <param name="relativePathWithoutSuffix">Application.persistent路径，后缀为.bytes或者.json，自动添加</param>
        /// <param name="useEncryption">是否加密</param>
        /// <returns></returns>
        public abstract T LoadData<T>(string relativePathWithoutSuffix, bool useEncryption) where T : ISaveData;

        /// <summary>
        /// 读取内存流数据
        /// </summary>
        /// <param name="memory">内存流</param>
        /// <param name="useEncryption">是否加密</param>
        public abstract T LoadData<T>(Stream memory, bool useEncryption) where T : ISaveData;

        /// <summary>
        /// 加密数据流
        /// </summary>
        protected Stream EncryptorStream(Stream stream)
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
        protected Stream DecryptorStream(Stream stream)
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
