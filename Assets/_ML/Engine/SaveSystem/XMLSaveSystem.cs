using System.IO;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.SaveSystem
{
    public class XMLSaveSystem : SaveSystem
    {
        public override T LoadData<T>(string relativePathWithoutSuffix, bool useEncryption)
        {
            if (!string.IsNullOrEmpty(relativePathWithoutSuffix))
            {
                string relativePath = relativePathWithoutSuffix + ".xml";
                string path = Path.Combine(SavePath, relativePath);
                if (File.Exists(path))
                {
                    using (FileStream fs = new FileStream(path, FileMode.Open))
                    {
                        Stream stream = fs;
                        if (useEncryption)
                        {
                            stream = DecryptorStream(fs);
                        }
                        DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                        T loadedData = (T)serializer.ReadObject(stream);
                        loadedData.Path = Path.GetDirectoryName(relativePath);
                        loadedData.SaveName = Path.GetFileNameWithoutExtension(relativePath);
                        stream.Close();
                        return loadedData;
                    }
                }
            }
            return null;
        }
        public override T LoadData<T>(Stream memory, bool useEncryption)
        {
            if (memory != null)
            {
                Stream stream = memory;
                if (useEncryption)
                {
                    stream = DecryptorStream(memory);
                }
                DataContractSerializer serializer = new DataContractSerializer(typeof(ISaveData));
                T loadedData = (T)serializer.ReadObject(stream);
                stream.Close();
                return loadedData;
            }
            return null;
        }
        public override void SaveData<T>(T data, bool useEncryption)
        {
            if (data!=null && !string.IsNullOrEmpty(data.SaveName) && data.IsDirty)
            {
                string path = Path.Combine(SavePath, data.Path, data.SaveName + ".xml");
                string directoryPath = Path.GetDirectoryName(path);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    Stream stream = fs;
                    if (useEncryption)
                    {
                        stream = EncryptorStream(fs);
                    }
                    DataContractSerializer serializer = new DataContractSerializer(data.GetType());
                    serializer.WriteObject(stream, data);
                    data.IsDirty = false;
                    stream.Close();
                }
            }
        }
    }
}
