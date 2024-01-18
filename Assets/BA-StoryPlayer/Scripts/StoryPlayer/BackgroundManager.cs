using BAStoryPlayer.DoTweenS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BAStoryPlayer
{
    public enum TransistionType
    {
        Immediate = 0,
        Fade
    }

    public class BackgroundManager : PlayerModule
    {
        [Header("References")]
        [SerializeField] private Image _imgBackground;

        private Dictionary<string, Sprite> _preloadedImages = new();
        public Dictionary<string, Sprite> PreloadedImages => _preloadedImages;

        private void Start()
        {
            if (_imgBackground == null)
            {
                _imgBackground = GetComponent<Image>();
            }
        }

        /// <summary>
        /// 设置背景 并优先适应宽度
        /// </summary>
        /// <param name="type">背景切换方式 首次无效</param>
        public void SetBackground(string imageName = null, TransistionType transition = TransistionType.Immediate)
        {
            if (imageName == null)
            {
                switch (transition)
                {
                    case TransistionType.Immediate:
                        _imgBackground.sprite = null;
                        _imgBackground.enabled = false;
                        break;
                    case TransistionType.Fade:
                        _imgBackground.DoColor(Color.black, StoryPlayer.Setting.TimeSwitchBackground).OnCompleted = () =>
                        {
                            _imgBackground.sprite = null;
                            _imgBackground.enabled = false;
                        };
                        break;
                }
                return;
            }

            if(PreloadedImages.ContainsKey(imageName))
            {
                SetBackground(PreloadedImages[imageName], transition);
            }
            else
            {
                Sprite sprite = Resources.Load<Sprite>(StoryPlayer.Setting.PathBackground + imageName);
                PreloadedImages.Add(imageName, sprite);
                SetBackground(sprite, transition);
            }
        }
        private void SetBackground(Sprite sprite, TransistionType transition)
        {
            if (sprite == null || sprite == _imgBackground.sprite)
            {
                return;
            }

            ResizeBackgroundToFitScreenWidth(sprite);

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
                    case TransistionType.Immediate:
                        {
                            _imgBackground.sprite = sprite;
                            break;
                        }
                    case TransistionType.Fade:
                        {
                            _imgBackground.DoColor(Color.black, StoryPlayer.Setting.TimeSwitchBackground / 2).OnCompleted = () =>
                            {
                                _imgBackground.sprite = sprite;
                                _imgBackground.DoColor(Color.white, StoryPlayer.Setting.TimeSwitchBackground / 2);
                            };
                            break;
                        }
                    default: break;
                }
            }
        }

        public void ResizeBackgroundToFitScreenWidth(Sprite sprite)
        {
            Vector2 size = sprite.rect.size;
            float ratio = size.y / size.x;
            size.x = StoryPlayer.CanvasRect.rect.width;
            size.y = size.x * ratio;
            _imgBackground.GetComponent<RectTransform>().sizeDelta = size;
        }

        public void SetBlurBackground(bool enable, TransistionType transition = TransistionType.Fade)
        {
            if (transition == TransistionType.Fade)
                _imgBackground.DoFloat("_Weight", enable ? 1 : 0, StoryPlayer.Setting.TimeBlurBackground);
            else if (transition == TransistionType.Immediate)
            {
                Material mat = new Material(_imgBackground.material);
                _imgBackground.material = mat;
                mat.SetFloat("_Weight", enable ? 1 : 0);
            }
        }

        public void ClearPreloadedImages()
        {
            PreloadedImages.Clear();
        }
    }
}
