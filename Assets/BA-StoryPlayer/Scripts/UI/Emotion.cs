using UnityEngine;
using UnityEngine.UI;
using BAStoryPlayer.DoTweenS;
using Spine.Unity;

namespace BAStoryPlayer.UI
{
    public class Emotion : MonoBehaviour
    {
        const float TIME_FADEOUT = 0.2f;

        public void Initlaize(Transform headLocator,Vector2 pos)
        {
            headLocator.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            var rect = GetComponent<RectTransform>();
            rect.SetParent(headLocator);
            rect.anchorMin = rect.anchorMax = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.anchoredPosition = pos;
        }

        public float GetClipLength() => GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.length;

        public void RunOnComplete()
        {
            var images = GetComponentsInChildren<Image>();
            for(int i = 0; i < images.Length; i++)
            {
                if (i == images.Length - 1)
                    images[i].DoAlpha(0, TIME_FADEOUT).OnCompleted = () => { Destroy(gameObject); };
                else 
                    images[i].DoAlpha(0, TIME_FADEOUT);
            }
        }

    }

}
