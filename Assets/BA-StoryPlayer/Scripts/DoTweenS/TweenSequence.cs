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
        private System.Collections.Generic.Queue<TweenUnit> _tweenQueue = new System.Collections.Generic.Queue<TweenUnit>();
        private bool _isPlaying = false;

        public void Append(TweenS tween)
        {
            TweenUnit tUnit = new TweenUnit(tween);
            _tweenQueue.Enqueue(tUnit);

            // 从tween列表中移除 防止重复被删
            DoTweenS.Remove(tween);
            tween.Stop();

            if (!_isPlaying)
            {
                _isPlaying = true;
                Next();
            }
        }
        public void Append(System.Action action)
        {
            TweenUnit tUnit = new TweenUnit(action);
            _tweenQueue.Enqueue(tUnit);

            if (!_isPlaying)
            {
                _isPlaying = true;
                Next();
            }
        }

        public void Wait(float time)
        {
            TweenUnit tUnit = new TweenUnit(time);
            _tweenQueue.Enqueue(tUnit);

            if (!_isPlaying)
            {
                _isPlaying = true;
                Next();
            }
        }

        void Next()
        {
            if (_tweenQueue.Count.Equals(0))
            {
                _isPlaying = false;
                return;
            }

            switch (_tweenQueue.Peek().UnitType)
            {
                case TweenUnitType.Cmd:
                    {
                        DoTweenS.MonoInstance.Delay(Next, _tweenQueue.Dequeue().Wait);
                        break;
                    }
                case TweenUnitType.Tween:
                    {
                        _tweenQueue.Dequeue().Tween.Resume().onCompleted += () =>
                        {
                            Next();
                        };
                        break;
                    }
                case TweenUnitType.Event:
                    {
                        _tweenQueue.Dequeue().Action?.Invoke();
                        Next();
                        break;
                    }
                default:return;
            }

        }
    }
}

