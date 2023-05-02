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
        public static TweenS DoMove_Anchored(this Transform transform, Vector2 target, float duration)
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
                    lerp = Mathf.Clamp(lerp - increment * Time.deltaTime, 0, 1);
                    yield return null;
                }

            }
            tween.RunOnComplete();
        }
        #endregion

        #region Transofrm操作
        public static TweenS DoScale(this Transform transform,Vector3 target,float duration)
        {
            MonoBehaviour mono = transform.GetComponent<MonoBehaviour>();
            TweenS tween = new TweenS(target, duration, transform);
            tween.type = TweenSType.Scale;
            tween.SetCoroutine(mono.StartCoroutine(DoLocalScale(mono, tween)));
            return tween;
        }
        public static  TweenS DoLocalScale(this Transform transform,Vector2 target,float duration)
        {
            return DoScale(transform, new Vector3(target.x, target.y, 1), duration);
        }
        static IEnumerator DoLocalScale(this MonoBehaviour mono,TweenS tween)
        {
            Vector3 target = (Vector3)tween.target,origin = tween.transform.localScale;
            float lerp = 0, increment = 1 / tween.duration;

            while(lerp != 1)
            {
                tween.transform.localScale = Vector3.Lerp(origin, target, lerp);
                lerp = Mathf.Clamp(lerp + increment * Time.deltaTime, 0, 1);
                yield return null;
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
            float valueX = 0, increment = destX / tween.duration;
            Vector3 origin = tween.transform.position;
            while(valueX != destX)
            {
                tween.transform.position = origin + new Vector3(func(valueX) / max * maxOffsetX,0,0);
                valueX = Mathf.Clamp(valueX + increment * Time.deltaTime, 0, destX);
                yield return null;
            }

            tween.RunOnComplete();
        }

        #endregion

        #region Spine组件颜色
        public static TweenS DoColor(this SkeletonGraphic skeletonGraphics,Color targetCol,float duration)
        {
            MonoBehaviour mono = skeletonGraphics.GetComponent<MonoBehaviour>();
            TweenS tween = new TweenS(targetCol, duration, mono.transform);
            tween.type = TweenSType.Color;
            tween.SetCoroutine(mono.StartCoroutine(DoColor_SkeletonGraphic(mono, tween)));
            return tween;
        }
        static IEnumerator DoColor_SkeletonGraphic(this MonoBehaviour mono,TweenS tween)
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

        #region Image组件
        public static TweenS DoAlpha(this Image image,float target,float duration)
        {
            MonoBehaviour mono = image.transform.GetComponent<MonoBehaviour>();
            TweenS tween = new TweenS(target, duration, image.transform);
            tween.type = TweenSType.Color;
            tween.SetCoroutine(mono.StartCoroutine(DoAlpha_Image(mono, tween)));
            return tween;
        }
        static IEnumerator DoAlpha_Image(this MonoBehaviour mono,TweenS tween)
        {
            float target = (float)tween.target;
            var image = tween.transform.GetComponent<Image>();
            float lerp = 0, increment = 1 / tween.duration;
            float origin = image.color.a;
            while (lerp != 1)
            {
                Color col = image.color;
                col.a = Mathf.Lerp(origin, target, lerp);
                image.color = col;
                lerp = Mathf.Clamp(lerp + increment * Time.deltaTime, 0, 1);
                yield return null;
            }

            tween.RunOnComplete();
        }

        public static TweenS DoColor(this Image image, Color target, float duration)
        {
            MonoBehaviour mono = image.transform.GetComponent<MonoBehaviour>();
            TweenS tween = new TweenS(target, duration, image.transform);
            tween.type = TweenSType.Color;
            tween.SetCoroutine(mono.StartCoroutine(DoColor_Image(mono, tween)));
            return tween;
        }
        static IEnumerator DoColor_Image(this MonoBehaviour mono, TweenS tween)
        {
            Color target = (Color)tween.target;
            var image = tween.transform.GetComponent<Image>();
            float lerp = 0, increment = 1 / tween.duration;
            Color origin = image.color;
            while (lerp != 1)
            {
                Color col = image.color;
                col = Color.Lerp(origin, target, lerp);
                image.color = col;
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
            tween.type = TweenSType.Material;
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

        #region 音频
        public static TweenS DoVolume(this AudioSource audio,float target, float duration)
        {
            MonoBehaviour mono = audio.GetComponent<MonoBehaviour>();
            TweenS tween = new TweenS(target, duration, audio.transform);
            tween.type = TweenSType.Audio;
            tween.SetCoroutine(mono.StartCoroutine(DoVolume(mono, tween)));
            return tween;

        }
        static IEnumerator DoVolume(this MonoBehaviour mono, TweenS tween)
        {
            AudioSource audio = tween.transform.GetComponent<AudioSource>();
            float target = (float)tween.target;
            float origin = audio.volume;
            float lerp = 0, increment = 1 / tween.duration;
            while (lerp != 1)
            {
                audio.volume = Mathf.Lerp(origin, target, lerp);
                lerp = Math.Clamp(lerp + increment * Time.deltaTime, 0, 1);
                yield return null;
            }

            tween.RunOnComplete();
        }
        #endregion
    }

}
