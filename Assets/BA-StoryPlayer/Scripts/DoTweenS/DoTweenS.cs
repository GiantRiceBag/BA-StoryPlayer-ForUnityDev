using System.Collections.Generic;

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

        #region 增删改查停
        public static void Add(TweenS tween)
        {
            CheckUniqueness(tween, true);
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
        /// 检查Tween对象唯一性
        /// </summary>
        /// <param name="transform">目标对象</param>
        /// <param name="kill">冲突时是否删除</param>
        /// <returns></returns>
        static bool CheckUniqueness(TweenS tween,bool kill = false)
        {
            int index = 0;
            foreach(var i in tweenList)
            {
                if (i.transform == tween.transform && i.type == tween.type)
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

}
