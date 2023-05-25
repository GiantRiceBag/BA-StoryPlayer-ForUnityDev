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
        internal int tid;
        internal float duration;
        internal Transform transform;
        internal MonoBehaviour monoBehaviour => transform.GetComponent<MonoBehaviour>();
        internal Coroutine coroutine;
        internal System.Func<MonoBehaviour,TweenS, System.Collections.IEnumerator> enumerator;
        internal object target;
        internal int time;
        internal string targetName;
        internal TweenSType type = TweenSType.Position;

        public System.Action onComplete;

        public TweenS(object target,float duration,Transform transform,TweenSType type){
            tid = DoTweenS.UsableTid;
            this.target = target;
            this.duration = duration;
            this.transform = transform;
            this.type = type;

            DoTweenS.Add(this);
        }
        public TweenS(object target, float duration, int times,Transform transform,TweenSType type) :this(target,duration,transform,type)
        {
            this.time = System.Math.Clamp(times, 1, int.MaxValue);
        }
        public TweenS(string targetName,object target, float duration,Transform transform, TweenSType type) : this(target, duration, transform, type)
        {
            this.targetName = targetName;
        }

        public TweenS OnComplete(System.Action action)
        {
            onComplete = action;
            return this;
        }
        public void Kill()
        {
            Stop();
            DoTweenS.Remove(this);
        }
        public TweenS Stop()
        {
            transform.GetComponent<MonoBehaviour>().StopCoroutine(coroutine);
            return this;
        }
        public TweenS Resume()
        {
            coroutine = monoBehaviour.StartCoroutine(enumerator(monoBehaviour, this));
            return this;
        }
        public void StartCoroutine(System.Func<MonoBehaviour, TweenS, System.Collections.IEnumerator> enumerator)
        {
            coroutine = monoBehaviour.StartCoroutine(enumerator(monoBehaviour, this));
            this.enumerator = enumerator;
        }
        public void RunOnComplete()
        {
            onComplete?.Invoke();
            Kill();
        }
    }

}
