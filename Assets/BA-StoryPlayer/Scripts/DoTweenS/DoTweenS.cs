using System.Collections.Generic;

namespace BAStoryPlayer.DoTweenS
{
    public class DoTweenS
    {
        private static DoTweenS instance;
        private static int usableTid = 0;
        private static List<TweenS> tweenList = new List<TweenS>();

        public static int UsableTid { get { return usableTid++; } }
        public static DoTweenS Instance
        {
            get
            {
                if (instance == null)
                    instance = new DoTweenS();

                return instance;
            }
        }

        #region ��ɾ�Ĳ�ͣ
        public static void Add(TweenS tween,bool checkUniqueness = true)
        {
            CheckIfUnique(tween, checkUniqueness);
            tweenList.Add(tween);
        }
        public static void Remove(TweenS tween)
        {
            if(tweenList.Contains(tween))
                tweenList.Remove(tween);
        }
        public static void KillAll()
        {
            if (tweenList.Count == 0) return;
            for (int i = tweenList.Count - 1; i >= 0; i--)
                tweenList[i].Kill();
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
        static bool CheckIfUnique(TweenS tween,bool kill = false)
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
