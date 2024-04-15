using System.IO;
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Xml.Linq;
using System;
using System.Linq;

namespace ML.Engine.SaveSystem
{
    public class XMLSaveSystem : SaveSystem
    {
        public override T LoadData<T>(string relativePath, bool useEncryption)
        {
            if (!string.IsNullOrEmpty(relativePath))
            {
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
                        using(StreamReader reader = new StreamReader(stream))
                        {
                            string xmlFromFile = reader.ReadToEnd();
                            XDocument xml = XDocument.Parse(xmlFromFile);
                            T loadedData = Activator.CreateInstance<T>();
                            Dictionary<string, PropertyInfo> properties = typeof(T).GetProperties().ToDictionary(p => p.Name);
                            Dictionary<string, FieldInfo> fields = typeof(T).GetFields().ToDictionary(p => p.Name);
                            foreach (XElement element in xml.Descendants())
                            {
                                if (properties.TryGetValue(element.Name.LocalName, out PropertyInfo property))
                                {
                                    property.SetValue(loadedData, Convert.ChangeType(element.Value, property.PropertyType));
                                }
                                if (fields.TryGetValue(element.Name.LocalName, out FieldInfo field))
                                {
                                    field.SetValue(loadedData, Convert.ChangeType(element.Value, field.FieldType));
                                }
                            }
                            loadedData.SavePath = Path.GetDirectoryName(relativePath);
                            loadedData.SaveName = Path.GetFileName(relativePath);
                            reader.Close();
                            stream.Close();
                            return loadedData;
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
                using (StreamReader reader = new StreamReader(stream))
                {
                    string xmlFromFile = reader.ReadToEnd();
                    XDocument xml = XDocument.Parse(xmlFromFile);
                    T loadedData = Activator.CreateInstance<T>();
                    Dictionary<string, PropertyInfo> properties = typeof(T).GetProperties().ToDictionary(p => p.Name);
                    Dictionary<string, FieldInfo> fields = typeof(T).GetFields().ToDictionary(p => p.Name);
                    foreach (XElement element in xml.Descendants())
                    {
                        if (properties.TryGetValue(element.Name.LocalName, out PropertyInfo property))
                        {
                            property.SetValue(loadedData, Convert.ChangeType(element.Value, property.PropertyType));
                        }
                        if (fields.TryGetValue(element.Name.LocalName, out FieldInfo field))
                        {
                            field.SetValue(loadedData, Convert.ChangeType(element.Value, field.FieldType));
                        }
                    }
                    reader.Close();
                    stream.Close();
                    return loadedData;
                }
            }
            return default(T);
        }
        public override void SaveData<T>(T data, bool useEncryption)
        {
            if (data!=null && !string.IsNullOrEmpty(data.SaveName) && data.IsDirty)
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
                    DataContractSerializer serializer = new DataContractSerializer(data.GetType());
                    serializer.WriteObject(stream, data);
                    data.IsDirty = false;
                    stream.Close();
                }
            }
        }
    }
}
