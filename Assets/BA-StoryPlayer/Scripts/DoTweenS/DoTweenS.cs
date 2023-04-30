using System.Collections.Generic;
using UnityEngine;
using System;

namespace BAStoryPlayer.DoTweenS
{
    public class DoTweenS
    {
        static DoTweenS _instance;
        static int usableTid = 0;
        public static int UsableTid
        {
            get
            {
                return usableTid++;
            }
        }
        public static DoTweenS Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DoTweenS();

                return _instance;
            }
        }
        static List<TweenS> tweenList = new List<TweenS>();

        #region ��ɾ�Ĳ�ͣ
        public static void Add(TweenS tween)
        {
            CheckUniqueness(tween.transform, true);
            tweenList.Add(tween);
        }
        public static void Remove(TweenS tween)
        {
            tweenList.Remove(tween);
        }
        public static void KillAll()
        {
            if (tweenList.Count == 0) return;
            StopAll();
            tweenList.Clear();
        }
        public static void Stop(int index)
        {
            tweenList[index].Stop();
        }
        public static void Kill(int index)
        {
            tweenList[index].Kill();
        }
        public static void StopAll()
        {
            foreach (var i in tweenList)
                i.Stop();
        }

        #endregion

        /// <summary>
        /// ���Tween����Ψһ��
        /// </summary>
        /// <param name="transform">Ŀ�����</param>
        /// <param name="kill">��ͻʱ�Ƿ�ɾ��</param>
        /// <returns></returns>
        static bool CheckUniqueness(Transform transform,bool kill = false)
        {
            int index = 0;
            foreach(var i in tweenList)
            {
                if (i.transform == transform)
                {
                    if (kill)
                        Kill(index);
                    return false;
                }
                index++;
            }

            return true;
        }
    }

}
