using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Net.Mime;
using UnityEngine.UI;

public class TimeScaleTest : MonoBehaviour
{
        [Range(0,10)]
        public float timeScale = 1f;
        public int updateCount = 0;
        int fixedUpdateCount = 0;
        public Text deltaTime, timeScaleTime,updateTime,fixedUpdateTime;
        private void Update()
        {
                deltaTime.text = Time.deltaTime.ToString();
                timeScaleTime.text = Time.unscaledDeltaTime.ToString();
                
                updateCount++;
                if (updateCount >= 500)
                {
                        updateCount = 0;
                }
                updateTime.text = updateCount.ToString();
        }

        private void FixedUpdate()
        {
                fixedUpdateCount++;
                if (fixedUpdateCount >= 500)
                {
                        fixedUpdateCount = 0;
                } 
                fixedUpdateTime.text = fixedUpdateCount.ToString();
        }

        private void OnValidate()
        {
                Time.timeScale = timeScale;
        }
}
