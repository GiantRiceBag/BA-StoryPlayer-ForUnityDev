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
            GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 848);
            transform.localScale = Vector3.one;
        }

        public void SetText(string text)
        {
            this.textmesh.text = $"<color=#767C88><size=40><b>   | </b></size></color>{text}";
            StartCoroutine(Delay());
        }

       IEnumerator Delay()
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
