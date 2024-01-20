using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace BAStoryPlayer.UI
{
    using DoTweenS;
    using global::BAStoryPlayer.Event;
    using State;
    using System;

    public class ButtonOption : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler
    {
        private const float WidthNormal = 0.701842f;
        private const float HeightNormal = 0.0924069f;

        private OptionButtonState State { get; set; }

        private OptionButtonState _normal = new Normal();
        private OptionButtonState _highlighted = new Highlighted();
        private OptionButtonState _pressed = new Pressed();
        private OptionButtonState _selected = new Selected();
        private OptionButtonState _disabled = new Disabled();

        private Image _image;
        private RectTransform _rectTransform;
        private TextMeshProUGUI _textMesh;

        private bool _isDisabled;
        private bool _isPointerHovering;
        private bool _isPointerPressed;
        private bool _isDesiredClicked;

        public bool IsDisabled
        {
            private set
            {
                _isDisabled = value;
                ReflashState();
            }
            get
            {
                return _isDisabled;
            }
        }
        public bool IsPointerHovering
        {
            private set
            {
                _isPointerHovering = value;
                ReflashState();
            }
            get
            {
                return _isPointerHovering;
            }
        }
        public bool IsPointerPressed
        {
            private set
            {
                _isPointerPressed = value;
                ReflashState();
            }
            get
            {
                return _isPointerPressed;
            }
        }
        public bool IsDesiredClicked
        {
            private set
            {
                _isDesiredClicked = value;
                ReflashState();
            }
            get
            {
                return _isDesiredClicked;
            }
        }

        public Image Image
        {
            get
            {
                if(_image == null)
                {
                    _image = transform.GetComponent<Image>();
                }
                return _image;
            }
        }
        public RectTransform RectTransform
        {
            get
            {
                if(_rectTransform == null)
                {
                    _rectTransform = GetComponent<RectTransform>();
                }
                return _rectTransform;
            }
        }
        public TextMeshProUGUI TextMesh
        {
            get
            {
                if(_textMesh == null)
                {
                    _textMesh = GetComponentInChildren<TextMeshProUGUI>();
                }
                return _textMesh;
            }
        }
        public OptionManager OptionManager
        { 
            get;
            private set;
        }

        public Action onClicked;

        private void Start()
        {
            SwitchState(_normal);

            _selected.onAnimationEnd = () =>
            {
                OptionManager?.FinishSelecting();
            };
        }

        private void ReflashState()
        {
            if (IsDisabled)
            {
                return;
            }

            if (IsDesiredClicked)
            {
                IsDisabled = true;
                SwitchState(_selected);
                onClicked?.Invoke();
                return;
            }

            if (IsPointerHovering)
            {
                if (IsPointerPressed)
                {
                    SwitchState(_pressed);
                }
                else
                {
                    SwitchState(_highlighted);
                }
            }
            else
            {
                SwitchState(_normal);
            }
        }
        private void SwitchState(OptionButtonState state)
        {
            State?.OnEnd(this);
            State = state;
            State?.OnStart(this);
        }

        public ButtonOption Initialize(OptionManager optionManager, OptionData optionData)
        {
            OptionManager = optionManager;

            GetComponent<RectTransform>().sizeDelta = new Vector2(
               WidthNormal * optionManager.StoryPlayer.CanvasRect.sizeDelta.x,
               HeightNormal * optionManager.StoryPlayer.CanvasRect.sizeDelta.y
           );
            transform.localScale = new Vector2(0.95f, 0.95f);
            transform.DoLocalScale(Vector2.one, 0.1f);

            TextMesh.text = optionData.text;

            onClicked += () =>
            {
                EventBus<OnPlayerSelected>.Raise(new OnPlayerSelected()
                {
                    storyUnits = optionData.storyUnits,
                    script = optionData.script
                });
                OptionManager?.StoryPlayer.AudioModule.PlaySoundButtonClick();
                OptionManager?.RevokeInteractablilty(transform);
            };

            return this;
        }
        public void Disable()
        {
            if (IsDisabled)
            {
                return;
            }

            IsDisabled = true;
            SwitchState(_disabled);
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            IsPointerHovering = true;
        }
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            IsPointerHovering = false;

            if (IsPointerPressed)
            {
                IsPointerPressed = false;
            }
        }
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (IsPointerHovering)
            {
                IsPointerPressed = true;
            }
        }
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (IsPointerHovering && IsPointerPressed)
            {
                IsDesiredClicked = true;
                IsPointerPressed = false;
            }
        }

        #region States
        protected abstract class OptionButtonState : State<ButtonOption>
        {
            public Action onAnimationEnd;
            
            protected void RunOnAnimationEnd()
            {
                onAnimationEnd?.Invoke();
            }
        }

        private class Normal : OptionButtonState
        {
            public override void OnStart(ButtonOption component)
            {
                component.RectTransform.DoScale(Vector3.one, 0.16667f).OnComplete(RunOnAnimationEnd);
            }
        }
        private class Highlighted : OptionButtonState
        {
            private Vector3 _highlightedScale = new Vector3(1.05f, 1.05f, 1.05f);

            public override void OnStart(ButtonOption component)
            {
                component.RectTransform.DoScale(_highlightedScale, 0.16667f).OnComplete(RunOnAnimationEnd);
            }
        }
        private class Pressed : OptionButtonState
        {
            private Vector3 _pressedScale = new Vector3(0.95f, 0.95f, 0.95f);

            public override void OnStart(ButtonOption component)
            {
                component.RectTransform.DoScale(_pressedScale, 0.16667f).OnComplete(RunOnAnimationEnd);
            }
        }
        private class Selected : OptionButtonState
        {
            public override void OnStart(ButtonOption component)
            {
                component.RectTransform.localScale = Vector3.one * 0.95f;

                component.RectTransform.DoLocalScale(Vector3.one * 1.2f, 0.833333f);
                component.Image.DoAlpha(0, 0.833333f);
                component.TextMesh.DoAlpha(0, 0.833333f).OnComplete(RunOnAnimationEnd);
            }
        }
        private class Disabled : OptionButtonState
        {
            public override void OnStart(ButtonOption component)
            {
                component.Image.DoAlpha(0, 0.13333f);
                component.TextMesh.DoAlpha(0, 0.13333f).OnComplete(RunOnAnimationEnd); ;
            }
        }
        #endregion
    }
}

namespace BAStoryPlayer.UI.State
{
    public interface IState<T>
    {
        public void OnStart(T component);
        public void OnUpdate(T component);
        public void OnEnd(T component);
    }

    public class State<T> : IState<T>
    {
        public virtual void OnEnd(T component) { }
        public virtual void OnStart(T component) { }
        public virtual void OnUpdate(T component) { }
    }
}