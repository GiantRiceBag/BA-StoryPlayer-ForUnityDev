using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BAStoryPlayer.DoTweenS;
using Unity.VisualScripting;
using BAStoryPlayer.Event;
using System.Linq;

namespace BAStoryPlayer.UI
{
    public class ButtonMenu : UILayerComponent
    {
        [SerializeField] private Color _colorSelectedBackground = new Color(0.2f,0.2f,0.2f,1);
        [SerializeField] private Color _colorSelectedText = Color.white;
        private Color _colorUnselectedText;
        [SerializeField] private TextMeshProUGUI _tmp;
        [SerializeField] private GameObject _subpanel;
        [Space]
        [SerializeField] private bool _isSelected = false;

        private Coroutine _crtDisableObject;

        public bool IsSelected
        {
            private set => _isSelected = value;
            get => _isSelected;
        }

        void Start()
        {
            if (_tmp == null)
            {
                _tmp = transform.GetComponentInChildren<TextMeshProUGUI>();
            }
            if (_subpanel == null)
            {
                _subpanel = transform.Find("SubPanel").gameObject;
            }

            _subpanel.SetActive(false);
            _colorUnselectedText = _tmp.color;
        }

        private void SwitchState(bool selected,bool immidiate = false)
        {
            if (selected)
            {
                _tmp.color = _colorSelectedText;
                GetComponent<Image>().color = _colorSelectedBackground;

                if (_crtDisableObject != null)
                {
                    StopCoroutine(_crtDisableObject);
                    _crtDisableObject = null;
                }
                _crtDisableObject = this.Delay(() =>
                {
                    IsSelected = false;
                    SwitchState(IsSelected);
                },5);

                _subpanel.SetActive(true);
                Image image_Subpanel = _subpanel.GetComponent<Image>();
                Image[] image_SubButton = _subpanel.GetComponentsInChildren<Image>();

                Color tempCol = image_Subpanel.color;
                tempCol.a = 0;
                image_Subpanel.color = tempCol;
                image_Subpanel.DoAlpha(1, 0.1f).onCompleted = () =>
                {
                    foreach (var i in image_SubButton)
                    {
                        tempCol = i.color;
                        tempCol.a = 0;
                        i.DoAlpha(1, 0.1f);
                    }
                };
            }
            else
            {
                _tmp.color = _colorUnselectedText;
                GetComponent<Image>().color = Color.white;

                if(_crtDisableObject != null)
                {
                    StopCoroutine(_crtDisableObject);
                    _crtDisableObject = null;
                }

                Image image_Subpanel = _subpanel.GetComponent<Image>();
                Image[] image_SubButton = _subpanel.GetComponentsInChildren<Image>().Where(e=>e!=image_Subpanel).ToArray();
                if (immidiate)
                {
                    image_Subpanel.color = new Color(image_Subpanel.color.r, image_Subpanel.color.g, image_Subpanel.color.b, 0);
                    _subpanel.SetActive(false);
                }
                else
                {
                    image_Subpanel.DoAlpha(0, 0.2f).OnCompleted(() =>
                    {
                        _subpanel.SetActive(false); 
                    });
                }

                foreach (var i in image_SubButton)
                {
                    if (immidiate)
                        i.color = new Color(i.color.r, i.color.g, i.color.b, 0);
                    else
                        i.DoAlpha(0, 0.1f);
                }
            }
        }

        private void OnEnable()
        {
            GetComponent<Button>().onClick.AddListener(OnClickEventHandler);
            EventBus<OnStartPlayingStory>.Binding.Add(OnStartPlayingStoryEventHandler);
        }
        private void OnDisable()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            IsSelected = false;
            SwitchState(IsSelected, true);
            EventBus<OnStartPlayingStory>.Binding.Remove(OnStartPlayingStoryEventHandler);
            GetComponent<Button>().onClick.RemoveListener(OnClickEventHandler);
        }

        public void PlaySound() 
        {
            StoryPlayer.AudioModule.PlaySoundButtonClick();
        }

        private void OnStartPlayingStoryEventHandler(OnStartPlayingStory data)
        {
            IsSelected = false;
            SwitchState(IsSelected, true);
        }
        private void OnClickEventHandler()
        {
            PlaySound();
            IsSelected = !IsSelected;
            SwitchState(IsSelected);
        }
    }
}
