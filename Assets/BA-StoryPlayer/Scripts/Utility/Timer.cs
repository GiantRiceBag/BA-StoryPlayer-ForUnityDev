using System;
using UnityEngine;

namespace BAStoryPlayer.Utility
{
    [Obsolete]
    public class Timer
    {
        public MonoBehaviour MonoBehaviour { private set; get; }

        public Timer(MonoBehaviour monoBehaviour)
        {
            MonoBehaviour = monoBehaviour;
        }

        public Coroutine Delay(System.Action action,float time)
        {
           return MonoBehaviour.StartCoroutine(CrtDelay(action, time));
        }

        public Coroutine Delay(Transform holder,System.Action action, float time)
        {
            return Delay(holder.GetComponent<MonoBehaviour>(), action, time);
        }

        public Coroutine Delay(MonoBehaviour holder, System.Action action, float time)
        {
            return holder.StartCoroutine(CrtDelay(action, time));
        }

        private System.Collections.IEnumerator CrtDelay(System.Action action,float time)
        {
            yield return new WaitForSeconds(time);
            action?.Invoke();
        }
    }
}
