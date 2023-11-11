using System;
using UnityEngine;

namespace BAStoryPlayer.DoTweenS
{
    public enum Ease
    {
        Linear,
        InQuad,
        OutQuad,
        InOutQuad,
        InCubic,
        OutCubic,
        InOutCubic,
        InQuart,
        OutQuart,
        InOutQuart,
        InQuint,
        OutQuint,
        InOutQuint,
        InSine,
        OutSine,
        InOutSine,
        InExpo,
        OutExpo,
        InOutExpo,
        InCirc,
        OutCirc,
        InOutCirc,
        InElastic,
        OutElastic,
        InOutElastic,
        InBack,
        OutBack,
        InOutBack,
        InBounce,
        OutBounce,
        InOutBounce
    }

    public static class EaseFunction
    {
        public static float Linear(float t) => t;

        public static float InQuad(float t) => t * t;
        public static float OutQuad(float t) => 1 - InQuad(1 - t);
        public static float InOutQuad(float t)
        {
            if (t < 0.5) return InQuad(t * 2) / 2;
            return 1 - InQuad((1 - t) * 2) / 2;
        }

        public static float InCubic(float t) => t * t * t;
        public static float OutCubic(float t) => 1 - InCubic(1 - t);
        public static float InOutCubic(float t)
        {
            if (t < 0.5) return InCubic(t * 2) / 2;
            return 1 - InCubic((1 - t) * 2) / 2;
        }

        public static float InQuart(float t) => t * t * t * t;
        public static float OutQuart(float t) => 1 - InQuart(1 - t);
        public static float InOutQuart(float t)
        {
            if (t < 0.5) return InQuart(t * 2) / 2;
            return 1 - InQuart((1 - t) * 2) / 2;
        }

        public static float InQuint(float t) => t * t * t * t * t;
        public static float OutQuint(float t) => 1 - InQuint(1 - t);
        public static float InOutQuint(float t)
        {
            if (t < 0.5) return InQuint(t * 2) / 2;
            return 1 - InQuint((1 - t) * 2) / 2;
        }

        public static float InSine(float t) => (float)-Mathf.Cos(t * Mathf.PI / 2);
        public static float OutSine(float t) => (float)Mathf.Sin(t * Mathf.PI / 2);
        public static float InOutSine(float t) => (float)(Mathf.Cos(t * Mathf.PI) - 1) / -2;

        public static float InExpo(float t) => (float)Mathf.Pow(2, 10 * (t - 1));
        public static float OutExpo(float t) => 1 - InExpo(1 - t);
        public static float InOutExpo(float t)
        {
            if (t < 0.5) return InExpo(t * 2) / 2;
            return 1 - InExpo((1 - t) * 2) / 2;
        }

        public static float InCirc(float t) => -((float)Mathf.Sqrt(1 - t * t) - 1);
        public static float OutCirc(float t) => 1 - InCirc(1 - t);
        public static float InOutCirc(float t)
        {
            if (t < 0.5) return InCirc(t * 2) / 2;
            return 1 - InCirc((1 - t) * 2) / 2;
        }

        public static float InElastic(float t) => 1 - OutElastic(1 - t);
        public static float OutElastic(float t)
        {
            float p = 0.3f;
            return (float)Mathf.Pow(2, -10 * t) * (float)Mathf.Sin((t - p / 4) * (2 * Mathf.PI) / p) + 1;
        }
        public static float InOutElastic(float t)
        {
            if (t < 0.5) return InElastic(t * 2) / 2;
            return 1 - InElastic((1 - t) * 2) / 2;
        }

        public static float InBack(float t)
        {
            float s = 1.70158f;
            return t * t * ((s + 1) * t - s);
        }
        public static float OutBack(float t) => 1 - InBack(1 - t);
        public static float InOutBack(float t)
        {
            if (t < 0.5) return InBack(t * 2) / 2;
            return 1 - InBack((1 - t) * 2) / 2;
        }

        public static float InBounce(float t) => 1 - OutBounce(1 - t);
        public static float OutBounce(float t)
        {
            float div = 2.75f;
            float mult = 7.5625f;

            if (t < 1 / div)
            {
                return mult * t * t;
            }
            else if (t < 2 / div)
            {
                t -= 1.5f / div;
                return mult * t * t + 0.75f;
            }
            else if (t < 2.5 / div)
            {
                t -= 2.25f / div;
                return mult * t * t + 0.9375f;
            }
            else
            {
                t -= 2.625f / div;
                return mult * t * t + 0.984375f;
            }
        }
        public static float InOutBounce(float t)
        {
            if (t < 0.5) return InBounce(t * 2) / 2;
            return 1 - InBounce((1 - t) * 2) / 2;
        }

        public static Func<float,float> Get(Ease type)
        {
            switch (type)
            {
                case Ease.Linear:
                default:
                    return Linear;
                case Ease.InQuad:
                    return InQuad;
                case Ease.OutQuad:
                    return OutQuad;
                case Ease.InOutQuad:
                    return InOutQuad;
                case Ease.InCubic:
                    return InCubic;
                case Ease.OutCubic:
                    return OutCubic;
                case Ease.InOutCubic:
                    return InOutCubic;
                case Ease.InQuart:
                    return InQuart;
                case Ease.OutQuart:
                    return OutQuart;
                case Ease.InOutQuart:
                    return InOutQuart;
                case Ease.InQuint:
                    return InQuint;
                case Ease.OutQuint:
                    return OutQuint;
                case Ease.InOutQuint:
                    return InOutQuint;
                case Ease.InSine:
                    return InSine;
                case Ease.OutSine:
                    return OutSine;
                case Ease.InOutSine:
                    return InOutSine;
                case Ease.InExpo:
                    return InExpo;
                case Ease.OutExpo:
                    return OutExpo;
                case Ease.InOutExpo:
                    return InOutExpo;
                case Ease.InCirc:
                    return InCirc;
                case Ease.OutCirc:
                    return OutCirc;
                case Ease.InOutCirc:
                    return InOutCirc;
                case Ease.InElastic:
                    return InElastic;
                case Ease.OutElastic:
                    return OutElastic;
                case Ease.InOutElastic:
                    return InOutElastic;
                case Ease.InBack:
                    return InBack;
                case Ease.OutBack:
                    return OutBack;
                case Ease.InOutBack:
                    return InOutBack;
                case Ease.InBounce:
                    return InBounce;
                case Ease.OutBounce:
                    return OutBounce;
                case Ease.InOutBounce:
                    return InOutBounce;
            }
        }
    }
}

