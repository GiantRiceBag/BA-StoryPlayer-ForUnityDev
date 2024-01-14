using System;
using System.Collections;
using UnityEngine;

namespace BAStoryPlayer
{
    public static class MonobehaviourExtension
    {
        public static Coroutine Delay(this Transform transform, Action action, float time)
        {
            return Delay(transform.GetComponent<MonoBehaviour>(), action, time);
        }
        public static Coroutine Delay(this GameObject gameObject, Action action, float time)
        {
            return Delay(gameObject.GetComponent<MonoBehaviour>(), action, time);
        }
        public static Coroutine Delay(this MonoBehaviour mono,Action action, float time)
        {
            return mono.StartCoroutine(
                    CrtDelay(action,time)
                );
        }

        private static IEnumerator CrtDelay(Action action, float time)
        {
            yield return new WaitForSeconds(time);
            action?.Invoke();
        }
    }
}