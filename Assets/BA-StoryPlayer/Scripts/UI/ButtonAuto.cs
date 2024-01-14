using UnityEngine;
using UnityEngine.UI;
using BAStoryPlayer.Event;

namespace BAStoryPlayer.UI
{
    public class ButtonAuto : UILayerComponent
    {
        [SerializeField] private Color _colorSelected = new Color(1, 0.8784314f, 0.4235294f);
        [SerializeField] private bool _isSelected = false;
        [SerializeField] private AudioClip _sound_Click;
        [SerializeField] private GameObject _objFlowLight;

        public AudioClip Sound_Click
        {
            get
            {
                if (_sound_Click == null)
                    _sound_Click = Resources.Load("Sound/Button_Click") as AudioClip;
                return _sound_Click;
            }
        }
        public GameObject FlowLight 
        {
            get
            {
                if(_objFlowLight == null)
                    _objFlowLight = transform.Find("FlowLight").gameObject;
                return _objFlowLight;
            }
         }

        void Start()
        {
            // TODO 临时使用 后期在优化弄的好看点
            GetComponent<Button>().onClick.AddListener(() =>
            {
                _isSelected = !_isSelected;
                StoryPlayer.IsAuto = _isSelected;
                StoryPlayer.AudioModule.Play(Sound_Click);

                if (_isSelected)
                {
                    GetComponent<Image>().color = _colorSelected;
                    FlowLight.SetActive(true);
                }
                else
                {
                    GetComponent<Image>().color = Color.white;
                    FlowLight.SetActive(false);
                }
            });

            EventBus<OnPlayerCanceledAuto>.Binding.Add(() =>
            {
                _isSelected = false;
                GetComponent<Image>().color = Color.white;
                FlowLight.SetActive(false);
            });
        }

        private void OnEnable()
        {
            _isSelected = StoryPlayer.IsAuto = false;
            FlowLight.SetActive(false);
            GetComponent<Image>().color = Color.white;
        }
    }
}
