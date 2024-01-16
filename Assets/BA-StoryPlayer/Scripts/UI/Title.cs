using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace BAStoryPlayer.UI
{
    public class Title : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _imgBannerline;
        [SerializeField] private Image _imgBaner;
        [SerializeField] private TextMeshProUGUI _txtTitle;
        [SerializeField] private TextMeshProUGUI _txtSubtitle;

        public BAStoryPlayer StoryPlayer { get; private set; }

        public void Initialize(BAStoryPlayer player,string title ,string subtitle = "")
        {
            StoryPlayer = player;
            _txtTitle.text = title;
            _txtSubtitle.text = subtitle;

            if (subtitle == "")
            {
                _txtSubtitle.gameObject.SetActive(false);
                _txtTitle.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }

            StoryPlayer.BackgroundModule.SetBlurBackground(true, BackgroundTransistionType.Instant);

            if (_imgBannerline == null)
            {
                _imgBannerline = transform.Find("BannerLine").GetComponent<Image>();
            }

            if (_imgBaner == null)
            {
                _imgBaner = transform.Find("TitleBanner").GetComponent<Image>();
            }

            if (_txtTitle == null)
            {
                _txtTitle = transform.Find("Text_Title").GetComponent<TextMeshProUGUI>();
            }

            if (_txtSubtitle == null)
            {
                _txtSubtitle = transform.Find("Text_Subtitle").GetComponent<TextMeshProUGUI>();
            }

            var rect = transform.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        public void RemoveBlurEffect()
        {
            StoryPlayer.BackgroundModule.SetBlurBackground(false);
        }

        public void RunOnComplete()
        {
            //  一般播放完标题就开始执行下一个单元
            StoryPlayer.ReadyToExecute(true);
            Destroy(gameObject);
        }
    }
}

