using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using TMPro;

namespace BAStoryPlayer.DoTweenS
{
    public static class DoTweenSExtension
    {
        #region 位移
        public static TweenS DoLocalMove(this Transform transform,Vector3 target,float duration)
        {
            MonoBehaviour mono = transform.GetComponent<MonoBehaviour>();
            TweenS tween = new TweenS(target, duration, mono, TweenSType.Position);
            tween.StartCoroutine(DoLocalMove);
            return tween;
        }
        private static IEnumerator DoLocalMove(this MonoBehaviour mono,TweenS tween)
        {
            Vector3 origin = mono.transform.localPosition, target = (Vector3)tween.target;
            float delta = 1 / tween.duration;

            while(tween.RawT != 1)
            {
                tween.RawT = Mathf.MoveTowards(tween.RawT, 1, delta * Time.deltaTime);
                mono.transform.localPosition = Vector3.Lerp(origin, target, tween.T);
                yield return null;
            }

            tween.RunOnComplete();
        }

        public static TweenS DoMove_Anchored(this Transform transform, Vector2 target, float duration)
        {
            MonoBehaviour mono = transform.GetComponent<MonoBehaviour>();
            TweenS tween = new TweenS(target, duration, mono, TweenSType.Position);
            tween.StartCoroutine(DoMove_Anchored) ;
            return tween;
        }

        static IEnumerator DoMove_Anchored(this MonoBehaviour mono, TweenS tween)
        {
            RectTransform rect = mono.transform.GetComponent<RectTransform>();

            float delta = (1 / tween.duration);
            Vector2 origin = rect.anchoredPosition;
            Vector2 target = (Vector2)tween.target;

            while (tween.RawT != 1)
            {
                tween.RawT = Mathf.MoveTowards(tween.RawT, 1, delta * Time.deltaTime);
                rect.anchoredPosition = Vector2.Lerp(origin, target, tween.T);
                yield return null;
            }

            tween.RunOnComplete();
        }

        public static TweenS DoBound_Anchored(this Transform transform, Vector2 target, float duration, int time = 1)
        {
            MonoBehaviour mono = transform.GetComponent<MonoBehaviour>();
            TweenS tween = new TweenS(target, duration, time, mono, TweenSType.Position);
            tween.StartCoroutine(DoBound_Anchored);
            return tween;
        }
        public static TweenS DoBound_Anchored_Relative(this Transform transform, Vector2 relativeTarget, float duration, int time = 1)
        {
            Vector2 target = transform.GetComponent<RectTransform>().anchoredPosition + relativeTarget;
            return DoBound_Anchored(transform,target,duration,time);
        }
        static IEnumerator DoBound_Anchored(this MonoBehaviour mono, TweenS tween)
        {
            RectTransform rect = mono.transform.GetComponent<RectTransform>();
            Vector2 origin = rect.anchoredPosition;
            Vector2 target = (Vector2)tween.target;
            float delta = 2 * tween.times * (1 / tween.duration);

            for (int i = 0; i < tween.times; i++)
            {
                while (tween.RawT != 1)
                {
                    tween.RawT = Mathf.MoveTowards(tween.RawT, 1, delta * Time.deltaTime);
                    rect.anchoredPosition = Vector2.Lerp(origin, target, tween.T);
                    yield return null;
                }

                while (tween.RawT != 0)
                {
                    tween.RawT = Mathf.MoveTowards(tween.RawT, 0, delta * Time.deltaTime);
                    rect.anchoredPosition = Vector2.Lerp(origin, target, tween.T);
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
            TweenS tween = new TweenS(target, duration, mono, TweenSType.Scale);
            tween.StartCoroutine(DoLocalScale);
            return tween;
        }
        public static  TweenS DoLocalScale(this Transform transform,Vector2 target,float duration)
        {
            return DoScale(transform, new Vector3(target.x, target.y, 1), duration);
        }
        static IEnumerator DoLocalScale(this MonoBehaviour mono,TweenS tween)
        {
            Vector3 target = (Vector3)tween.target,origin = mono.transform.localScale;
            float delta = 1 / tween.duration;

            while(tween.RawT != 1)
            {
                tween.RawT = Mathf.MoveTowards(tween.RawT, 1, delta * Time.deltaTime);
                mono.transform.localScale = Vector3.Lerp(origin, target, tween.T);
                yield return null;
            }

            tween.RunOnComplete();
        }

        #endregion

        #region X轴摇晃
        public static TweenS DoShakeX(this Transform transform,float maxOffsetX,float duration,int times = 1)
        {
            MonoBehaviour mono = transform.GetComponent<MonoBehaviour>();
            TweenS tween = new TweenS(maxOffsetX, duration, times, mono,TweenSType.Position);
            tween.StartCoroutine(DoShakeX);
            return tween;
        }
        static IEnumerator DoShakeX(this MonoBehaviour mono,TweenS tween)
        {
            float destX = tween.times * 2 * Mathf.PI;
            Func<float, float> func = (x) =>
             {
                 return Mathf.Sin(x) * (-x + destX);
             };

            float maxOffsetX = Mathf.Abs((float)tween.target);
            float max = func(Mathf.PI / 2);
            float valueX = 0, increment = destX / tween.duration;
            Vector3 origin = mono.transform.localPosition;
            while(valueX != destX)
            {
                mono.transform.localPosition = origin + new Vector3(func(valueX) / max * maxOffsetX,0,0);
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
            TweenS tween = new TweenS(targetCol, duration, mono,TweenSType.Color);
            tween.StartCoroutine(DoColor_SkeletonGraphic);
            return tween;
        }
        static IEnumerator DoColor_SkeletonGraphic(this MonoBehaviour mono,TweenS tween)
        {
            Color targetCol = (Color)tween.target;
            var skeletonGraphic = mono.GetComponent<SkeletonGraphic>();
            Color originCol = skeletonGraphic.color;

            float delta = 1 / tween.duration;
            while(tween.RawT != 1)
            {
                tween.RawT = Mathf.MoveTowards(tween.RawT, 1, delta * Time.deltaTime);
                skeletonGraphic.color = Color.Lerp(originCol, targetCol, tween.T);
                yield return null;
            }

            tween.RunOnComplete();
        }
        #endregion

        #region Image组件
        public static TweenS DoAlpha(this Image image,float target,float duration)
        {
            MonoBehaviour mono = image.transform.GetComponent<MonoBehaviour>();
            TweenS tween = new TweenS(target, duration, mono,TweenSType.Color);
            tween.StartCoroutine(DoAlpha_Image);
            return tween;
        }
        static IEnumerator DoAlpha_Image(this MonoBehaviour mono,TweenS tween)
        {
            float target = (float)tween.target;
            var image = mono.GetComponent<Image>();
            float delta = 1 / tween.duration;
            float origin = image.color.a;

            while (tween.RawT != 1)
            {
                Color col = image.color;
                tween.RawT = Mathf.MoveTowards(tween.RawT, 1, delta * Time.deltaTime);
                col.a = Mathf.Lerp(origin, target, tween.T);
                image.color = col;

                yield return null;
            }

            tween.RunOnComplete();
        }

        public static TweenS DoColor(this Image image, Color target, float duration)
        {
            MonoBehaviour mono = image.transform.GetComponent<MonoBehaviour>();
            TweenS tween = new TweenS(target, duration, mono,TweenSType.Color);
            tween.StartCoroutine(DoColor_Image);
            return tween;
        }
        static IEnumerator DoColor_Image(this MonoBehaviour mono, TweenS tween)
        {
            Color target = (Color)tween.target;
            var image = mono.GetComponent<Image>();
            float delta = 1 / tween.duration;
            Color origin = image.color;

            while (tween.RawT != 1)
            {
                Color col = image.color;

                tween.RawT = Mathf.MoveTowards(tween.RawT, 1, delta * Time.deltaTime);
                col = Color.Lerp(origin, target, tween.T);
                image.color = col;
                
                yield return null;
            }

            tween.RunOnComplete();
        }
        #endregion

        #region Textmesh
        public static TweenS DoAlpha(this TextMeshProUGUI textmesh, float target, float duration)
        {
            MonoBehaviour mono = textmesh.transform.GetComponent<MonoBehaviour>();
            TweenS tween = new TweenS(target, duration, mono, TweenSType.Color);
            tween.StartCoroutine(DoAlpha_TextmeshProUGUI);
            return tween;
        }
        static IEnumerator DoAlpha_TextmeshProUGUI(this MonoBehaviour mono, TweenS tween)
        {
            float target = (float)tween.target;
            var textmesh = mono.GetComponent<TextMeshProUGUI>();
            float delta = 1 / tween.duration;
            float origin = textmesh.color.a;

            while (tween.RawT != 1)
            {
                Color col = textmesh.color;
                tween.RawT = Mathf.MoveTowards(tween.RawT, 1, delta * Time.deltaTime);
                col.a = Mathf.Lerp(origin, target, tween.T);
                textmesh.color = col;

                yield return null;
            }

            tween.RunOnComplete();
        }
        #endregion

        #region 材质
        public static TweenS DoFloat(this Image image,string propertyName,float target,float duration)
        {
            MonoBehaviour mono = image.GetComponent<MonoBehaviour>();
            TweenS tween = new TweenS(propertyName, target, duration,mono,TweenSType.Material);
            tween.StartCoroutine(DoFloat_Image_Material);
            return tween;

        }
        static IEnumerator DoFloat_Image_Material(this MonoBehaviour mono,TweenS tween)
        {
            Image image = mono.GetComponent<Image>();
            Material mat = new Material(image.material);
            image.material = mat;
            float target = (float)tween.target;
            float origin = mat.GetFloat(tween.targetName);
            float delta = 1 / tween.duration;

            while(tween.RawT != 1)
            {
                tween.RawT = Mathf.MoveTowards(tween.RawT,1,delta * Time.deltaTime);
                mat.SetFloat(tween.targetName, Mathf.Lerp(origin, target, tween.T));
                yield return null;
            }

            tween.RunOnComplete();
        }
        #endregion

        #region 音频
        public static TweenS DoVolume(this AudioSource audio,float target, float duration)
        {
            MonoBehaviour mono = audio.GetComponent<MonoBehaviour>();
            TweenS tween = new TweenS(target, duration, mono,TweenSType.Audio);
            tween.StartCoroutine(DoVolume);
            return tween;

        }
        static IEnumerator DoVolume(this MonoBehaviour mono, TweenS tween)
        {
            AudioSource audio = mono.GetComponent<AudioSource>();
            float target = (float)tween.target;
            float origin = audio.volume;
            float delta = 1 / tween.duration;

            while (tween.RawT != 1)
            {
                tween.RawT = Mathf.MoveTowards(tween.RawT, 1, delta * Time.deltaTime);
                audio.volume = Mathf.Lerp(origin, target, tween.T);
                yield return null;
            }

            tween.RunOnComplete();
        }
        #endregion

        #region 旋度
        public static TweenS DoEuler(this Transform transform,Vector3 target,float duration)
        {
            MonoBehaviour mono = transform.GetComponent<MonoBehaviour>();
            TweenS tween = new TweenS(target, duration, mono, TweenSType.Rotation);
            tween.StartCoroutine(DoEulerAngle);
            return tween;
        }

        static IEnumerator DoEulerAngle(this MonoBehaviour mono,TweenS tween)
        {
            Quaternion origin = mono.transform.rotation;
            Quaternion target = Quaternion.Euler((Vector3)tween.target);
            float delta = 1 / tween.duration;

            while(tween.RawT != 1)
            {
                tween.RawT = Mathf.MoveTowards(tween.RawT, 1, delta * Time.deltaTime);
                mono.transform.rotation = Quaternion.Lerp(origin,target,tween.T);
                yield return null;
            }

            tween.RunOnComplete();
        }
        #endregion
    }
}
