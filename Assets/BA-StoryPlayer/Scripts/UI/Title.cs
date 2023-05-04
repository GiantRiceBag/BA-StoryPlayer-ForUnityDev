using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace BAStoryPlayer.UI
{
    public class Title : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Image image_Bannerline;
        [SerializeField] Image image_Baner;
        [SerializeField] TextMeshProUGUI text_Title;
        [SerializeField] TextMeshProUGUI text_Subtitle;

        private void Start()
        {
            BAStoryPlayerController.Instance.StoryPlayer.UIModule.SetBlurBackgroup(true);

            if (image_Bannerline == null)
                image_Bannerline = transform.Find("BannerLine").GetComponent<Image>();
            if (image_Baner == null)
                image_Baner = transform.Find("TitleBanner").GetComponent<Image>();
            if (text_Title == null)
                text_Title = transform.Find("Text_Title").GetComponent<TextMeshProUGUI>();
            if (text_Subtitle == null)
                text_Subtitle = transform.Find("Text_Subtitle").GetComponent<TextMeshProUGUI>();

            var rect = transform.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0);
            rect.anchorMax = new Vector2(0.5f, 0);
            rect.pivot = new Vector2(0.5f, 0);
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        public void Initialize(string title ,string subtitle)
        {
            text_Title.text = title;
            text_Subtitle.text = subtitle;
        }

        public void RemoveBlurEffect()
        {
            BAStoryPlayerController.Instance.StoryPlayer.UIModule.SetBlurBackgroup(false);
        }

        public void RunOnComplete()
        {
            //  一般播放完标题就开始执行下一个单元
            BAStoryPlayerController.Instance.StoryPlayer.ReadyToNext(true);
            Destroy(gameObject);
        }
    }
}

