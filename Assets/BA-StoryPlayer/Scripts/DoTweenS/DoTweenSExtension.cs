using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;

namespace BAStoryPlayer.DoTweenS
{
    public static class DoTweenSExtension
    {
        #region 基于锚点的移动
        public static TweenS DMove_Anchored(this Transform transform, Vector2 target, float duration)
        {
            MonoBehaviour mono = transform.GetComponent<MonoBehaviour>();
            TweenS tween = new TweenS(target, duration, transform);
            tween.SetCoroutine(mono.StartCoroutine(DoMove_Anchored(mono, tween)));
            return tween;
        }
        static IEnumerator DoMove_Anchored(this MonoBehaviour mono, TweenS tween)
        {
            RectTransform rect = tween.transform.GetComponent<RectTransform>();

            float lerp = 0, increment = (1 / tween.duration);
            Vector2 origin = rect.anchoredPosition;
            Vector2 target = (Vector2)tween.target;

            while (rect.anchoredPosition != target)
            {
                rect.anchoredPosition = Vector2.Lerp(origin, target, lerp);
                lerp = Mathf.Clamp(lerp + increment * Time.deltaTime, 0, 1f);
                yield return null;
            }

            tween.RunOnComplete();
        }

        #endregion

        #region 基于锚点的跳跃
        public static TweenS DoBound_Anchored(this Transform transform, Vector2 target, float duration, int time = 1)
        {
            MonoBehaviour mono = transform.GetComponent<MonoBehaviour>();
            TweenS tween = new TweenS(target, duration, time, transform);
            tween.SetCoroutine(mono.StartCoroutine(DoBound_Anchored(mono, tween)));
            return tween;
        }
        public static TweenS DoBound_Anchored_Relative(this Transform transform, Vector2 relativeTarget, float duration, int time = 1)
        {
            Vector2 target = transform.GetComponent<RectTransform>().anchoredPosition + relativeTarget;
            return DoBound_Anchored(transform,target,duration,time);
        }
        static IEnumerator DoBound_Anchored(this MonoBehaviour mono, TweenS tween)
        {
            RectTransform rect = tween.transform.GetComponent<RectTransform>();
            Vector2 origin = rect.anchoredPosition;
            Vector2 target = (Vector2)tween.target;
            float increment = 2 * tween.time * (1 / tween.duration);
            float lerp = 0;
            for (int i = 0; i < tween.time; i++)
            {
                while (lerp != 1)
                {
                    rect.anchoredPosition = Vector2.Lerp(origin, target, lerp);
                    lerp = Mathf.Clamp(lerp + increment * Time.deltaTime, 0, 1);
                    yield return null;
                }

                while (lerp != 0)
                {
                    rect.anchoredPosition = Vector2.Lerp(origin, target, lerp);
                    lerp = Mathf.Clamp(lerp + increment * Time.deltaTime, 0, 1);
                    yield return null;
                }

            }
            tween.RunOnComplete();
        }
        #endregion

        #region X轴摇晃
        public static TweenS DoShakeX(this Transform transform,float maxOffsetX,float duration,int time = 1)
        {
            MonoBehaviour mono = transform.GetComponent<MonoBehaviour>();
            TweenS tween = new TweenS(maxOffsetX, duration, time, transform);
            tween.SetCoroutine(mono.StartCoroutine(DoShakeX(mono, tween)));
            return tween;
        }
        static IEnumerator DoShakeX(this MonoBehaviour mono,TweenS tween)
        {
            float destX = tween.time * 2 * Mathf.PI;
            Func<float, float> func = (x) =>
             {
                 return Mathf.Sin(x) * (-x + destX);
             };

            float maxOffsetX = Mathf.Abs((float)tween.target);
            float max = func(Mathf.PI / 2);
            float lerp = 0, increment = destX / tween.duration;
            Vector3 origin = tween.transform.position;
            while(lerp != destX)
            {
                tween.transform.position = origin + new Vector3(func(lerp) / max * maxOffsetX,0,0);
                lerp = Mathf.Clamp(lerp + increment * Time.deltaTime, 0, 1);
                yield return null;
            }

            tween.RunOnComplete();
        }

        #endregion

        #region 颜色
        public static TweenS DoColor(this SkeletonGraphic skeletonGraphics,Color targetCol,float duration)
        {
            MonoBehaviour mono = skeletonGraphics.GetComponent<MonoBehaviour>();
            TweenS tween = new TweenS(targetCol, duration, mono.transform);
            tween.SetCoroutine(mono.StartCoroutine(DoColor(mono, tween)));
            return tween;
        }
        static IEnumerator DoColor(this MonoBehaviour mono,TweenS tween)
        {
            Color targetCol = (Color)tween.target;
            var skeletonGraphic = tween.transform.GetComponent<SkeletonGraphic>();
            Color originCol = skeletonGraphic.color;
            float lerp = 0, increment = 1 / tween.duration;
            while(lerp != 1)
            {
                skeletonGraphic.color = Color.Lerp(originCol, targetCol, lerp);
                lerp = Mathf.Clamp(lerp + increment * Time.deltaTime, 0, 1);
                yield return null;
            }

            tween.RunOnComplete();
        }
        #endregion

        #region 材质
        public static TweenS DoFloat(this Image image,string propertyName,float target,float duration)
        {
            MonoBehaviour mono = image.GetComponent<MonoBehaviour>();
            TweenS tween = new TweenS(propertyName, target, duration,image.transform);
            tween.SetCoroutine(mono.StartCoroutine(DoFloat_Image_Material(mono, tween)));
            return tween;

        }
        static IEnumerator DoFloat_Image_Material(this MonoBehaviour mono,TweenS tween)
        {
            Image image = tween.transform.GetComponent<Image>();
            Material mat = image.material;
            mat = GameObject.Instantiate(mat);
            image.material = mat;
            float target = (float)tween.target;
            float origin = mat.GetFloat(tween.targetName);
            float lerp = 0, increment = 1 / tween.duration;
            while(mat.GetFloat(tween.targetName) != target)
            {
                mat.SetFloat(tween.targetName, Mathf.Lerp(origin, target, lerp));
                lerp = Math.Clamp(lerp + increment * Time.deltaTime, 0, 1);
                yield return null;
            }

            tween.RunOnComplete();
        }
        #endregion
    }

}
