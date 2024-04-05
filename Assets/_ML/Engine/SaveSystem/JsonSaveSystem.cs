using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

namespace ML.Engine.SaveSystem
{
    public class JsonSaveSystem : SaveSystem
    {
        public override T LoadData<T>(string relativePath, bool useEncryption)
        {
            if (!string.IsNullOrEmpty(relativePath))
            {
                string path = Path.Combine(SavePath, relativePath);
                if (File.Exists(path))
                {
                    using(FileStream fs = new FileStream(path, FileMode.Open))
                    {
                        Stream stream = fs;
                        if (useEncryption)
                        {
                            stream = DecryptorStream(fs);
                        }
                        using(StreamReader reader = new StreamReader(stream))
                        {
                            string jsonFromFile = reader.ReadToEnd();
                            T objFromFile = JsonConvert.DeserializeObject<T>(jsonFromFile);
                            objFromFile.SavePath = Path.GetDirectoryName(relativePath);
                            objFromFile.SaveName = Path.GetFileName(relativePath);
                            reader.Close();
                            stream.Close();
                            return objFromFile;
                        }
                    }
                }
            }
            return default(T);
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
                using(StreamReader reader = new StreamReader(stream))
                {
                    string jsonFromFile = reader.ReadToEnd();
                    T objFromFile = JsonConvert.DeserializeObject<T>(jsonFromFile);
                    reader.Close();
                    stream.Close();
                    return objFromFile;
                }
            }
            return default(T);
        }
        public override void SaveData<T>(T data, bool useEncryption)
        {
            if (data != null && !string.IsNullOrEmpty(data.SaveName) && data.IsDirty)
            {
                string path = Path.Combine(SavePath, data.SavePath, data.SaveName);
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
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        string json = JsonConvert.SerializeObject(data);
                        writer.Write(json);
                        data.IsDirty = false;
                        writer.Close();
                        stream.Close();
                    }
                }
            }
        }
    }
}