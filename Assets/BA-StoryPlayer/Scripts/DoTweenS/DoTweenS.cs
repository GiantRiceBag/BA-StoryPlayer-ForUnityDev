using System.Collections.Generic;
using UnityEngine;

namespace BAStoryPlayer.DoTweenS
{
    public class DoTweenS
    {
        private static DoTweenS s_instance;
        private static GameObject s_gameObject;
        private static MonoBehaviour s_mono;
        private static int s_usableTid = 0;
        private static List<TweenS> s_tweenList = new List<TweenS>();

        public static int UsableTid { get { return s_usableTid++; } }
        public static DoTweenS Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new DoTweenS();
                }

                return s_instance;
            }
        }
        public static MonoBehaviour MonoInstance
        {
            get
            {
                if(s_gameObject == null || s_mono == null)
                {
                    GameObject obj = new GameObject("[DoTweenS]",typeof(DoTweenSRuntime));
                    obj.hideFlags = HideFlags.HideInHierarchy;

                    s_gameObject = obj;
                    s_mono = s_gameObject.GetComponent<MonoBehaviour>();
                    Object.DontDestroyOnLoad(s_gameObject);
                }
                return s_mono;
            }
        }

        #region 增删改查停
        public static void Add(TweenS tween,bool checkUniqueness = true)
        {
            CheckIfUnique(tween, checkUniqueness);
            s_tweenList.Add(tween);
        }
        public static void Remove(TweenS tween)
        {
            if (s_tweenList.Contains(tween))
            {
                s_tweenList.Remove(tween);
            }
        }
        public static void KillAll()
        {
            if (s_tweenList.Count == 0) return;
            for (int i = s_tweenList.Count - 1; i >= 0; i--)
                s_tweenList[i].Kill();
        }
        public static void Stop(int index)
        {
            s_tweenList[index].Stop();
        }
        public static void Kill(int index)
        {
            s_tweenList[index].Kill();
        }
        public static void StopAll()
        {
            foreach (var i in s_tweenList)
                i.Stop();
        }

        #endregion

        /// <summary>
        /// 检查Tween对象唯一性
        /// </summary>
        /// <param name="transform">目标对象</param>
        /// <param name="kill">冲突时是否删除</param>
        /// <returns></returns>
        private static bool CheckIfUnique(TweenS tween,bool kill = false)
        {
            int index = 0;
            foreach(var i in s_tweenList)
            {
                if (i.mono == tween.mono && i.type == tween.type)
                {
                    if (kill)
                    {
                        Kill(index);
                    }
                    return false;
                }
                index++;
            }

            return true;
        }
    }

    public class DoTweenSRuntime : MonoBehaviour
    {

    }
}
