using UnityEngine;
namespace BAStoryPlayer.Utility
{
    public class Timer : BSingleton<Timer>
    {
        protected override void Awake()
        {
            base.Awake();
            gameObject.hideFlags = HideFlags.HideInHierarchy;
        }

        public static Coroutine Delay(System.Action action,float time)
        {
           return Instance.StartCoroutine(CDelay(action, time));
        }

        public static Coroutine Delay(Transform holder,System.Action action, float time)
        {
            return Delay(holder.GetComponent<MonoBehaviour>(), action, time);
        }

        public static Coroutine Delay(MonoBehaviour holder, System.Action action, float time)
        {
            return holder.StartCoroutine(CDelay(action, time));
        }

        static System.Collections.IEnumerator CDelay(System.Action action,float time)
        {
            yield return new WaitForSeconds(time);
            action?.Invoke();
        }
    }

}
