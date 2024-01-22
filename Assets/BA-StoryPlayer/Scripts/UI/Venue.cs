using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace BAStoryPlayer.UI
{
    using DoTweenS;
    using System.Reflection;

    public class Venue : MonoBehaviour
    {
        [SerializeField] RectTransform banner;
        [SerializeField] TextMeshProUGUI textmesh;

        private void Start()
        {
            var rect = GetComponent<RectTransform>();

            rect.anchorMin = rect.anchorMax = Vector2.up;
            rect.anchoredPosition = new Vector2(0, -232);
            transform.localScale = Vector3.one;
        }

        public void SetText(string text)    
        {
            this.textmesh.text = $"<color=#767C88><size=40><b>   | </b></size></color>{text}";
            StartCoroutine(CrtDelay());

            PlayAnimation();
        }

       IEnumerator CrtDelay()
        {
            yield return null;
            banner.sizeDelta  = new Vector2(textmesh.GetComponent<RectTransform>().sizeDelta.x + 30,banner.sizeDelta.y);
        }

        public void RunOnComplete()
        {
            Destroy(gameObject);
        }

        private void PlayAnimation()
        {
            TweenSequence sequence = new();

            Image image = banner.GetComponent<Image>();
            image.color = new Color(image.color.r, image.color.g, image.color.b,0);
            textmesh.color = new Color(textmesh.color.r, textmesh.color.g, textmesh.color.b, 0); ;

            sequence.Append(() =>
            {
                textmesh.DoAlpha(1, 0.85f);
            });
            sequence.Append(image.DoAlpha(0.69804f, 0.85f));
            sequence.Wait(1.666667f);
            sequence.Append(() =>
            {
                textmesh.DoAlpha(0, 0.7f);
            });
            sequence.Append(image.DoAlpha(0, 0.7f));
            sequence.Wait(0.1f);
            sequence.Append(RunOnComplete);
        }
    }

}
