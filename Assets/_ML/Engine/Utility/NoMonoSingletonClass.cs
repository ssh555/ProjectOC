using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ML.Engine.Utility
{
    public class NoMonoSingletonClass<T> where T : class
    {
        private static T _instance = null;

        public NoMonoSingletonClass()
        {
            if(_instance == null)
            {
                _instance = this as T;
            }
        }

        ~NoMonoSingletonClass()
        {
            if(_instance == this)
            {
                _instance = null;
            }
        }

        public static T Instance
        {
            get
            {
                return _instance;
            }
        }
    }

}
