using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ML.Engine.SaveSystem
{
    /// <summary>
    /// 对应一个存档文件夹下的SaveConfig文件
    /// 存此存档的相关数据(不是实际数据)
    /// 存档路径Path + 存档名称name == 存档所在文件夹
    /// 在此文件夹下有一个SaveConfig文件，就是存储的SaveDataFolder数据
    /// 其余文件为存档数据文件
    /// </summary>
    public class SaveDataFolder : ISaveData
    {
        /// <summary>
        /// 此存档的名称
        /// </summary>
        public string Name;

        /// <summary>
        /// 存档创建时间
        /// </summary>
        public string CreateTime;

        /// <summary>
        /// 最近修改时间
        /// </summary>
        public string LastSaveTime;

        /// <summary>
        /// 存档文件名称-对应的数据结构类型全名字符串的映射表
        /// SaveName -> Path+Name+SaveName+(.bytes|.json)为实际的存储绝对路径
        /// </summary>
        public Dictionary<string, string> FileMap = new Dictionary<string, string>();
    }
}