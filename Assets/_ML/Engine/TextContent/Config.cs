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
        public static Language language => Manager.GameManager.Instance.language;
        [SerializeField]
        public static Platform platform => Manager.GameManager.Instance.platform;
        [SerializeField]
        public static InputDevice inputDevice => Manager.GameManager.Instance.inputDevice;
    }

}
