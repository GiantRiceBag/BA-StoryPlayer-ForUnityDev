using UnityEngine;
using TMPro;
using System.Collections;

namespace BAStoryPlayer.UI
{
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
    }

}
