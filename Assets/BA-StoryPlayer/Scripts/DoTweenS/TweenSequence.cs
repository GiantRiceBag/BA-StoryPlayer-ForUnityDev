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
        public TweenUnitType unitType;
        public TweenS tween;
        public System.Action action;
        public float wait;

        public TweenUnit(TweenS tween)
        {
            this.tween = tween;
            unitType = TweenUnitType.Tween;
        }
        public TweenUnit(float wait)
        {
            wait = Mathf.Abs(wait);
            this.wait = wait;
            unitType = TweenUnitType.Cmd;
        }
        public TweenUnit(System.Action action)
        {
            this.action = action;
            unitType = TweenUnitType.Event;
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

            switch (tweenQueue.Peek().unitType)
            {
                case TweenUnitType.Cmd:
                    {
                        Timer.Delay(Next, tweenQueue.Dequeue().wait);
                        break;
                    }
                case TweenUnitType.Tween:
                    {
                        tweenQueue.Dequeue().tween.Resume().onComplete += () =>
                        {
                            Next();
                        };
                        break;
                    }
                case TweenUnitType.Event:
                    {
                        tweenQueue.Dequeue().action?.Invoke();
                        Next();
                        break;
                    }
                default:return;
            }

        }
    }
}

