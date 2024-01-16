using BAStoryPlayer.DoTweenS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BAStoryPlayer
{
    public enum BackgroundTransistionType
    {
        Instant = 0,
        Smooth
    }

    public class BackgroundManager : PlayerModule
    {
        [Header("References")]
        [SerializeField] private Image _imgBackground;

        private void Start()
        {
            if (_imgBackground == null)
            {
                _imgBackground = GetComponent<Image>();
            }
        }


        /// <summary>
        /// ���ñ��� ��������Ӧ���
        /// </summary>
        /// <param name="url">���URL</param>
        /// <param name="type">�����л���ʽ �״���Ч</param>
        public void SetBackground(string url = null, BackgroundTransistionType transition = BackgroundTransistionType.Instant)
        {
            if (url == null)
            {
                switch (transition)
                {
                    case BackgroundTransistionType.Instant:
                        _imgBackground.sprite = null;
                        _imgBackground.enabled = false;
                        break;
                    case BackgroundTransistionType.Smooth:
                        _imgBackground.DoColor(Color.black, StoryPlayer.Setting.TimeSwitchBackground).OnCompleted = () =>
                        {
                            _imgBackground.sprite = null;
                            _imgBackground.enabled = false;
                        };
                        break;
                }
                return;
            }

            Sprite sprite = Resources.Load<Sprite>(StoryPlayer.Setting.PathBackground + url);

            if (sprite == null)
            {
                Debug.LogError($"û����·�� {StoryPlayer.Setting.PathBackground + url}  �ҵ����� [{url}]  ");
                return;
            }

            Vector2 size = sprite.rect.size;
            float ratio = size.y / size.x;
            size.x = StoryPlayer.CanvasRect.rect.width;
            size.y = size.x * ratio;

            _imgBackground.GetComponent<RectTransform>().sizeDelta = size;
            if (!_imgBackground.enabled)
            {
                _imgBackground.enabled = true;
                _imgBackground.sprite = sprite;
                _imgBackground.color = Color.white;
            }
            else
            {
                switch (transition)
                {
                    case BackgroundTransistionType.Instant:
                        {
                            _imgBackground.sprite = sprite;
                            break;
                        }
                    case BackgroundTransistionType.Smooth:
                        {
                            _imgBackground.DoColor(Color.black, StoryPlayer.Setting.TimeSwitchBackground / 2).OnCompleted = () =>
                            {
                                _imgBackground.sprite = sprite;
                                _imgBackground.DoColor(Color.white, StoryPlayer.Setting.TimeSwitchBackground / 2);
                            };
                            break;
                        }
                    default: return;
                }
            }
        }


        public void SetBlurBackground(bool enable, BackgroundTransistionType transition = BackgroundTransistionType.Smooth)
        {
            if (transition == BackgroundTransistionType.Smooth)
                _imgBackground.DoFloat("_Weight", enable ? 1 : 0, StoryPlayer.Setting.TimeBlurBackground);
            else if (transition == BackgroundTransistionType.Instant)
            {
                Material mat = new Material(_imgBackground.material);
                _imgBackground.material = mat;
                mat.SetFloat("_Weight", enable ? 1 : 0);
            }
        }
    }
}
