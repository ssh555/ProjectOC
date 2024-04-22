using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.UI
{
    public interface ISelected
    {
        public ISelected LeftUI { get; set; }
        public ISelected RightUI { get; set; }
        public ISelected UpUI { get; set; }
        public ISelected DownUI { get; set; }
       
    }

}
