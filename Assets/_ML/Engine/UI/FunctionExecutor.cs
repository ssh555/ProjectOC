using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections;
namespace ML.Engine.UI
{
    public class FunctionExecutor<T> where T : IEnumerator
    {
        private List<Func<T>> FunctionList = new List<Func<T>>();
        private Action onAllFunctionsCompleted; // 所有函数执行完毕后的回调函数

        // 构造函数
        public FunctionExecutor(List<Func<T>> FunctionList, Action onAllFunctionsCompleted)
        {
            this.FunctionList = FunctionList;
            this.onAllFunctionsCompleted = onAllFunctionsCompleted;
        }

        //执行
        public IEnumerator Execute()
        {
            IEnumerator[] enumerators = new IEnumerator[FunctionList.Count];
            for (int i = 0; i < FunctionList.Count; i++)
            {
                enumerators[i] = FunctionList[i].Invoke();
            }
            for (int i = 0; i < FunctionList.Count; i++)
            {
                yield return enumerators[i];
            }


            this.onAllFunctionsCompleted.Invoke();
        }
    }
}

