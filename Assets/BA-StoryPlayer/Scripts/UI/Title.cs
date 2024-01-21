using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace BAStoryPlayer.UI
{
    using DoTweenS;

    public class Title : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _imgBannerline;
        [SerializeField] private Image _imgBaner;
        [SerializeField] private TextMeshProUGUI _txtTitle;
        [SerializeField] private TextMeshProUGUI _txtSubtitle;

        public BAStoryPlayer StoryPlayer { get; private set; }

        public void InitializeAndPlay(BAStoryPlayer player,string title ,string subtitle = "")
        {
            StoryPlayer = player;
            _txtTitle.text = title;
            _txtSubtitle.text = subtitle;

            if (subtitle == "")
            {
                _txtSubtitle.gameObject.SetActive(false);
                _txtTitle.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }

            StoryPlayer.BackgroundModule.SetBlurBackground(true, TransistionType.Immediate);

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

            PlayAnimation();
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

        private void PlayAnimation()
        {
            TweenSequence sequence = new();

            _txtTitle.rectTransform.localScale = Vector3.one;
            _imgBannerline.color = new Color(_imgBannerline.color.r, _imgBannerline.color.g, _imgBannerline.color.b, 0.5f);
            _imgBaner.color = new Color(_imgBaner.color.r, _imgBaner.color.g, _imgBaner.color.b, 0);
            _txtSubtitle.color = new Color(_txtSubtitle.color.r, _txtSubtitle.color.g, _txtSubtitle.color.b,0);
            _txtTitle.color = new Color(_txtTitle.color.r, _txtTitle.color.g, _txtTitle.color.b, 0);

            sequence.Append(_imgBannerline.DoAlpha(1, 0.33333f));
            sequence.Wait(0.3333f);
            sequence.Append(_imgBaner.DoAlpha(1, 0.68333f));
            sequence.Wait(0.05f);
            if (!string.IsNullOrEmpty(_txtSubtitle.text))
            {
                sequence.Append(_txtSubtitle.DoAlpha(1, 0.433333f));
                sequence.Append(_txtTitle.DoAlpha(1, 0.33333f));
                sequence.Wait(1.73333f);
            }
            else
            {
                sequence.Append(() =>
                {
                    _txtTitle.rectTransform.DoScale(Vector3.one * 1.3f, 1.3f).SetEase(Ease.OutCirc);
                });
                sequence.Append(_txtTitle.DoAlpha(1, 1.3f));
                sequence.Wait(0.73333f);
            }
            
            sequence.Append(RemoveBlurEffect);
            sequence.Append(() =>
            {
                _imgBannerline.DoAlpha(0, 0.766667f);
                _txtTitle.DoAlpha(0, 0.766667f);
                if (_txtSubtitle.gameObject.activeSelf)
                {
                    _txtSubtitle.DoAlpha(0, 0.766667f);
                }
            });
            sequence.Append(_imgBaner.DoAlpha(0, 0.8f));
            sequence.Append(RunOnComplete);
        }
    }
}

