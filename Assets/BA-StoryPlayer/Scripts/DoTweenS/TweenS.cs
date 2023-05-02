using System;
using UnityEngine;

namespace BAStoryPlayer.DoTweenS
{
    public enum TweenSType
    {
        Vector,
        Color,
        Material,
        Scale,
        Audio
    }

    public class TweenS
    {
        internal int tid;
        internal float duration;
        internal Transform transform;
        internal Coroutine coroutine;
        internal object target;
        internal int time;
        internal string targetName;
        internal TweenSType type = TweenSType.Vector;

        public Action onComplete;

        public TweenS(object target,float duration,Transform transform,Coroutine coroutine = null){
            tid = DoTweenS.UsableTid;
            this.target = target;
            this.duration = duration;
            this.transform = transform;
            this.coroutine = coroutine;

            DoTweenS.Add(this);
        }
        public TweenS(object target, float duration, int time,Transform transform, Coroutine coroutine = null):this(target,duration,transform,coroutine)
        {
            this.time = Math.Clamp(time, 1, int.MaxValue);
        }
        public TweenS(string targetName,object target, float duration,Transform transform, Coroutine coroutine = null) : this(target, duration, transform, coroutine)
        {
            this.targetName = targetName;
        }

        public TweenS OnComplete(Action action)
        {
            onComplete = action;
            return this;
        }
        public void Kill()
        {
            Stop();
            DoTweenS.Remove(this);
        }
        public void Stop()
        {
            transform.GetComponent<MonoBehaviour>().StopCoroutine(coroutine);
        }
        public void SetCoroutine(Coroutine coroutine)
        {
            this.coroutine = coroutine;
        }
        public void RunOnComplete()
        {
            onComplete?.Invoke();
            Kill();
        }
    }

}
