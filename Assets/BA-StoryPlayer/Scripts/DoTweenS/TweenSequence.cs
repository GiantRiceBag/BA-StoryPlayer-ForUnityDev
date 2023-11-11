using UnityEngine;
using BAStoryPlayer.Utility;
namespace BAStoryPlayer.DoTweenS
{
    public enum TweenUnitType
    {
        Cmd,
        Tween,
        Event
    }

    public class TweenUnit
    {
        public TweenUnitType UnitType;
        public TweenS Tween;
        public System.Action Action;
        public float Wait;

        public TweenUnit(TweenS tween)
        {
            this.Tween = tween;
            UnitType = TweenUnitType.Tween;
        }
        public TweenUnit(float wait)
        {
            wait = Mathf.Abs(wait);
            this.Wait = wait;
            UnitType = TweenUnitType.Cmd;
        }
        public TweenUnit(System.Action action)
        {
            this.Action = action;
            UnitType = TweenUnitType.Event;
        }
    }

    public class TweenSequence
    {
        System.Collections.Generic.Queue<TweenUnit> tweenQueue = new System.Collections.Generic.Queue<TweenUnit>();
        bool isPlaying = false;
        public void Append(TweenS tween)
        {
            TweenUnit tUnit = new TweenUnit(tween);
            tweenQueue.Enqueue(tUnit);

            // 从tween列表中移除 防止重复被删
            DoTweenS.Remove(tween);
            tween.Stop();

            if (!isPlaying)
            {
                isPlaying = true;
                Next();
            }
        }

        public void Append(System.Action action)
        {
            TweenUnit tUnit = new TweenUnit(action);
            tweenQueue.Enqueue(tUnit);

            if (!isPlaying)
            {
                isPlaying = true;
                Next();
            }
        }

        public void Wait(float time)
        {
            TweenUnit tUnit = new TweenUnit(time);
            tweenQueue.Enqueue(tUnit);

            if (!isPlaying)
            {
                isPlaying = true;
                Next();
            }
        }

        void Next()
        {
            if (tweenQueue.Count.Equals(0))
                return;

            switch (tweenQueue.Peek().UnitType)
            {
                case TweenUnitType.Cmd:
                    {
                        Timer.Delay(Next, tweenQueue.Dequeue().Wait);
                        break;
                    }
                case TweenUnitType.Tween:
                    {
                        tweenQueue.Dequeue().Tween.Resume().OnCompleted += () =>
                        {
                            Next();
                        };
                        break;
                    }
                case TweenUnitType.Event:
                    {
                        tweenQueue.Dequeue().Action?.Invoke();
                        Next();
                        break;
                    }
                default:return;
            }

        }
    }
}

