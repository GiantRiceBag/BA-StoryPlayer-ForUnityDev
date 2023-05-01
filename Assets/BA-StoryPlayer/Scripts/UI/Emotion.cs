using UnityEngine;
using UnityEngine.UI;
using BAStoryPlayer.DoTweenS;

namespace BAStoryPlayer.UI
{
    public class Emotion : MonoBehaviour
    {
        const float TIME_TRANSITION = 0.2f;

        public void Initlaize(Transform parent,Vector2 pos)
        {
            var rect = GetComponent<RectTransform>();
            rect.SetParent(parent);
            //rect.anchorMin = rect.anchorMax = Vector2.zero;
            rect.localScale = Vector2.one;
            rect.anchoredPosition = pos;
        }

        public void RunOnComplete()
        {
            //TODO ·¢ËÍÊÂ¼þ
            var images = GetComponentsInChildren<Image>();
            for(int i = 0; i < images.Length; i++)
            {
                if (i == images.Length - 1)
                    images[i].DoAlpha(0, TIME_TRANSITION).onComplete = () => { Destroy(gameObject); };
                else 
                    images[i].DoAlpha(0, TIME_TRANSITION);
            }
        }

    }

}
