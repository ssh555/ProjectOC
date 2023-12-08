using ML.Engine.BuildingSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ML.Engine.TextContent
{
    public static class Config
    {
        public enum Language
        {
            Chinese,
            English,
        }
        public enum Platform
        {
            Windows,
        }
        public enum InputDevice
        {
            Keyboard,
            XBOX,
        }

        [SerializeField]
        public static Language language => Test_BuildingManager.Instance.language;
        [SerializeField]
        public static Platform platform => Test_BuildingManager.Instance.platform;
        [SerializeField]
        public static InputDevice inputDevice => Test_BuildingManager.Instance.inputDevice;
    }

}
