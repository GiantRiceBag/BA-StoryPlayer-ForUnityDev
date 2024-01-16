using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using BAStoryPlayer.DoTweenS;
using BAStoryPlayer.Event;
using TMPro;
using System;

namespace BAStoryPlayer.UI
{
    public class ButtonOption : MonoBehaviour,IPointerExitHandler,IPointerEnterHandler,IPointerDownHandler,IPointerUpHandler
    {
        private Animator _animator;
        [SerializeField] AudioClip _soundClick;

        private const float WidthNormal = 0.701842f;
        private const float HeightNormal = 0.0924069f;

        [Obsolete] public int OptionID { get; private set; }
        public bool Clicked { get; private set; }
        public bool IsPointerOnButton { get; private set; }

        public OptionManager OptionManager { get; private set; }

        [Obsolete]
        public void Initialize(OptionManager optionManager,int optionID,string text)
        {
            OptionManager = optionManager;
            OptionID = optionID;
            GetComponentInChildren<TextMeshProUGUI>().text = text;

            if (_soundClick == null)
            {
                _soundClick = Resources.Load("Sound/Button_Click") as AudioClip;
            }

            _animator = GetComponent<Animator>();
            _animator.enabled = false;

            GetComponent<RectTransform>().sizeDelta = new Vector2(
                   WidthNormal * optionManager.StoryPlayer.CanvasRect.sizeDelta.x,
                   HeightNormal * optionManager.StoryPlayer.CanvasRect.sizeDelta.y
               );

            transform.localScale = new Vector2(0.95f, 0.95f);
            transform.DoLocalScale(Vector2.one, 0.1f).OnCompleted = () => {
                _animator.enabled = true;
            };

            GetComponent<Button>().onClick.AddListener(() =>
            {
                if (Clicked)
                {
                    return;
                }
                Clicked = true;
                optionManager.StoryPlayer.AudioModule.Play(_soundClick);
                _animator.SetBool("Interactable", false);
                optionManager.RevokeInteractablilty(transform);
                EventBus<OnPlayerSelected>.Raise(new OnPlayerSelected()
                {
                    scriptGourpID = optionManager.StoryPlayer.GroupID,
                    selectionGroup = OptionID
                });
            });
        }
        public void Initialize(OptionManager optionManager, OptionData optionData)
        {
            OptionManager = optionManager;
            GetComponentInChildren<TextMeshProUGUI>().text = optionData.text;

            if (_soundClick == null)
            {
                _soundClick = Resources.Load("Sound/Button_Click") as AudioClip;
            }

            _animator = GetComponent<Animator>();
            _animator.enabled = false;

            GetComponent<RectTransform>().sizeDelta = new Vector2(
                   WidthNormal * optionManager.StoryPlayer.CanvasRect.sizeDelta.x,
                   HeightNormal * optionManager.StoryPlayer.CanvasRect.sizeDelta.y
               );

            transform.localScale = new Vector2(0.95f, 0.95f);
            transform.DoLocalScale(Vector2.one, 0.1f).OnCompleted = () => {
                _animator.enabled = true;
            };

            GetComponent<Button>().onClick.AddListener(() =>
            {
                if (Clicked)
                {
                    return;
                }
                Clicked = true;
                optionManager.StoryPlayer.AudioModule.Play(_soundClick);
                _animator.SetBool("Interactable", false);
                optionManager.RevokeInteractablilty(transform);
                EventBus<OnPlayerSelected>.Raise(new OnPlayerSelected()
                {
                    storyUnits = optionData.storyUnits,
                    script = optionData.script
                });
            });
        }
        public void RunOnComplete()
        {
            OptionManager.FinishSelecting();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            IsPointerOnButton = false;

            _animator.SetBool("Normal", true);
            _animator.SetBool("Pressed", false);
            _animator.SetBool("Selected", false);
            _animator.SetBool("Highlighted", false);
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            IsPointerOnButton = true;

            _animator.SetBool("Normal", false);
            _animator.SetBool("Pressed", false);
            _animator.SetBool("Selected", false);
            _animator.SetBool("Highlighted", true);
        }
        public void OnPointerDown(PointerEventData eventData)
        {   
            if(IsPointerOnButton == true)
            {
                _animator.SetBool("Normal", false);
                _animator.SetBool("Pressed", true);
                _animator.SetBool("Selected", false);
                _animator.SetBool("Highlighted", false);
            }
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            if (IsPointerOnButton == true)
            {
                _animator.SetBool("Normal", false);
                _animator.SetBool("Pressed", false);
                _animator.SetBool("Selected", true);
                _animator.SetBool("Highlighted", false);
            }
        }
    }

}
