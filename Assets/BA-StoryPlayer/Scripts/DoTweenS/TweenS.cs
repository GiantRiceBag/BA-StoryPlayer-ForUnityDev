using System;
using UnityEngine;

namespace BAStoryPlayer.DoTweenS
{
    public enum TweenSType
    {
        Position,
        Color,
        Material,
        Scale,
        Audio,
        Rotation
    }

    public class TweenS
    {
        private float _t = 0;

        internal int tid;
        internal float duration;
        internal MonoBehaviour mono;
        internal Coroutine coroutine;
        internal Func<MonoBehaviour,TweenS, System.Collections.IEnumerator> enumerator;
        internal object target;
        internal int times;
        internal string targetName;
        internal TweenSType type = TweenSType.Position;

        internal Func<float,float> easeFunction;

        public float T
        {
            get
            {
                return easeFunction(_t);
            }
        }
        public float RawT
        {
            set
            {
                _t = value;
            }
            get
            {
                return _t;
            }
        }

        public Action OnCompleted;

        public TweenS(object target,float duration,MonoBehaviour mono,TweenSType type)
        {
            tid = DoTweenS.UsableTid;
            this.target = target;
            this.duration = duration;
            this.mono = mono;
            this.type = type;

            easeFunction = EaseFunction.Linear;

            DoTweenS.Add(this);
        }
        public TweenS(object target, float duration, int times, MonoBehaviour mono,TweenSType type) :this(target,duration,mono,type)
        {
            this.times = Mathf.Clamp(times, 1, int.MaxValue);
        }
        public TweenS(string targetName,object target, float duration, MonoBehaviour mono, TweenSType type) : this(target, duration, mono, type)
        {
            this.targetName = targetName;
        }

        public TweenS OnComplete(Action action)
        {
            OnCompleted = action;
            return this;
        }
        public void Kill()
        {
            Stop();
            DoTweenS.Remove(this);
        }
        public TweenS Stop()
        {
            mono.StopCoroutine(coroutine);
            return this;
        }
        public TweenS Resume()
        {
            coroutine = mono.StartCoroutine(enumerator(mono, this));
            return this;
        }

        public TweenS SetEase(Ease type)
        {
            easeFunction = EaseFunction.Get(type);
            return this;
        }

        public void StartCoroutine(Func<MonoBehaviour, TweenS, System.Collections.IEnumerator> enumerator)
        {
            coroutine = mono.StartCoroutine(enumerator(mono, this));
            this.enumerator = enumerator;
        }
        public void RunOnComplete()
        {
            OnCompleted?.Invoke();
            Kill();
        }
    }
}
