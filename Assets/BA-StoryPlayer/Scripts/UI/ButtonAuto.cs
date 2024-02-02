using UnityEngine;
using UnityEngine.UI;
using BAStoryPlayer.Event;

namespace BAStoryPlayer.UI
{
    public class ButtonAuto : UILayerComponent
    {
        [SerializeField] private Color _colorSelected = new Color(1, 0.8784314f, 0.4235294f);
        [SerializeField] private bool _isSelected = false;
        [SerializeField] private Material _material;

        private readonly int RectOffsetPID = Shader.PropertyToID("_FlowingLightRectTransformOffset");

        public Material Material
        {
            get
            {
                if(_material == null) 
                { 
                    _material = new Material(GetComponent<Image>().material);
                    GetComponent<Image>().material = _material;
                }
                return _material;
            }
        }

        private void Start()
        {
            _material = new Material(GetComponent<Image>().material);
            GetComponent<Image>().material = _material;
        }

        private void OnEnable()
        {
            GetComponent<Button>().onClick.AddListener(OnClickEventHandler);
            EventBus<OnCanceledAuto>.Binding.Add(OnCancelAutoEventHandler);
            _isSelected = StoryPlayer.IsAuto = false;
            SetEnableFlowingLight(false);
            GetComponent<Image>().color = Color.white;

            Canvas.willRenderCanvases += OnRenderCanvasesEventHandler;
        }
        private void OnDisable()
        {
            GetComponent<Button>().onClick.RemoveListener(OnClickEventHandler);
            EventBus<OnCanceledAuto>.Binding.Remove(OnCancelAutoEventHandler);
            Canvas.willRenderCanvases -= OnRenderCanvasesEventHandler;
        }

        private void OnCancelAutoEventHandler(OnCanceledAuto data)
        {
            _isSelected = false;
            GetComponent<Image>().color = Color.white;
            SetEnableFlowingLight(false);
        }

        public void SetEnableFlowingLight(bool enable)
        {
            Material.SetInt("_EnableFlowingLight", enable ? 1 : 0);
        }

        private void OnClickEventHandler()
        {
            _isSelected = !_isSelected;
            StoryPlayer.IsAuto = _isSelected;
            StoryPlayer.AudioModule.PlaySoundButtonClick();

            if (_isSelected)
            {
                GetComponent<Image>().color = _colorSelected;
                SetEnableFlowingLight(true);
            }
            else
            {
                GetComponent<Image>().color = Color.white;
                SetEnableFlowingLight(false);
            }
        }
        private void OnRenderCanvasesEventHandler()
        {
           _material.SetVector(
               RectOffsetPID,
               new Vector4(transform.localPosition.x, transform.localPosition.y - StoryPlayer.CanvasRect.sizeDelta.y / 2)
               );
        }
    }
}
