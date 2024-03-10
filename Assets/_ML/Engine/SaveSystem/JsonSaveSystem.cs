using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

namespace ML.Engine.SaveSystem
{
    public class JsonSaveSystem : SaveSystem
    {
        public override ISaveData LoadData(string relativePathWithoutSuffix, bool useEncryption)
        {
            if (!string.IsNullOrEmpty(relativePathWithoutSuffix))
            {
                string path = Path.Combine(SavePath, relativePathWithoutSuffix + ".json");
                if (File.Exists(path))
                {
                    using FileStream fs = new FileStream(path, FileMode.Open);
                    Stream stream = fs;
                    if (useEncryption)
                    {
                        stream = DecryptorStream(fs);
                    }
                    using StreamReader reader = new StreamReader(stream);
                    string jsonFromFile = reader.ReadToEnd();

                    ISaveData objFromFile = JsonConvert.DeserializeObject<ISaveData>(jsonFromFile);
                    reader.Close();
                    stream.Close();
                    return objFromFile;
                }
            }
            return null;
        }
        public override ISaveData LoadData(Stream memory, bool useEncryption)
        {
            if (memory != null)
            {
                Stream stream = memory;
                if (useEncryption)
                {
                    stream = DecryptorStream(memory);
                }
                using StreamReader reader = new StreamReader(stream);
                string jsonFromFile = reader.ReadToEnd();

                ISaveData objFromFile = JsonConvert.DeserializeObject<ISaveData>(jsonFromFile);
                reader.Close();
                stream.Close();
                return objFromFile;
            }
            return null;
        }
        public override void SaveData(ISaveData data, bool useEncryption)
        {
            if (data != null && !string.IsNullOrEmpty(data.Path) && data.IsDirty)
            {
                string path = Path.Combine(SavePath, data.Path + ".json");
                string directoryPath = Path.GetDirectoryName(path);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                using FileStream fs = new FileStream(path, FileMode.Create);
                Stream stream = fs;
                if (useEncryption)
                {
                    stream = EncryptorStream(fs);
                }
                using StreamWriter writer = new StreamWriter(stream);
                string json = JsonConvert.SerializeObject(data);
                writer.Write(json);

                writer.Close();
                stream.Close();
                data.IsDirty = false;
            }
        }
    }
}