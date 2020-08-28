using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace EasyLocalization
{
    //借助EditorApplication.update实现编辑器状态下的协程
    //使用的话 yield return xxx xxx需要继承于Iwait的接口
    public static class EditorCoroutine
    {
        private static List<IEnumerator> ieList = new List<IEnumerator>();
        private static List<Action> actionList = new List<Action>();
        private static List<int> removeList = new List<int>();

        private static bool isTicking = false;

        static void EditorUpdate()
        {
            if (ieList.Count <= 0) return;
            if (!isTicking) return;
            for (int i = 0; i < ieList.Count; i++)
            {
                bool moveNext = true;
                if (ieList[i].Current != null)
                {
                    //查看是否只自定义的wait事件
                    //我并不清楚，unity源码中是怎么做的，所以没有什么办法兼容。。。
                    Type type = ieList[i].Current.GetType();
                    if(type == typeof(WaitForSomeFrame) || type == typeof(WaitForSomeTime))
                    {
                        IWait wait = (IWait)ieList[i].Current;
                        if (wait.isDone())
                        {
                            moveNext = ieList[i].MoveNext();
                        }
                    }
                    else
                    {
                        moveNext = ieList[i].MoveNext();
                    }
                }
                else
                {
                    moveNext = ieList[i].MoveNext();
                }
                if(!moveNext)
                {
                    removeList.Add(i);
                    if(actionList.Count > i && actionList[i] != null)
                    {
                        actionList[i].Invoke();
                    }
                }
            }
            //是否有需要删除的元素
            if (removeList.Count <= 0) return;
            removeList.Reverse();
            for (int i = 0; i < removeList.Count; i++)
            {
                ieList.RemoveAt(removeList[i]);
                actionList.RemoveAt(removeList[i]);
            }
            removeList.Clear();
            if (ieList.Count <= 0)
            {
                EditorApplication.update -= EditorUpdate;
                isTicking = false;
                Debug.Log("coroutine stop");
            }
        }

        public static void StartCoroutine(IEnumerator ie, Action action = null)
        {
            ieList.Add(ie);
            actionList.Add(action);
            if (!isTicking)
                EditorApplication.update += EditorUpdate;
            isTicking = true;
        }
    }

    interface IWait
    {
        bool isDone();
    }

    class WaitForSomeFrame : IWait
    {
        private int frame = 0;

        public WaitForSomeFrame(int frame)
        {
            this.frame = frame;
        }

        public bool isDone()
        {
            frame--;
            return frame <= 0;
        }
    }

    //姑且认为一帧就是0.033秒吧(●ˇ∀ˇ●)
    class WaitForSomeTime : IWait
    {
        private float time = 0;
        public WaitForSomeTime(float time)
        {
            this.time = time;
        }

        public bool isDone()
        {
            time -= 0.033f;
            return time <= 0;
        }
    }
}
