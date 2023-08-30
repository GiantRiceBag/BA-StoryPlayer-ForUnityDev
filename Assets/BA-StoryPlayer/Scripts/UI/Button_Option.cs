using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using BAStoryPlayer.DoTweenS;
using BAStoryPlayer.Event;
using TMPro;

namespace BAStoryPlayer.UI
{
    public class Button_Option : MonoBehaviour,IPointerExitHandler,IPointerEnterHandler,IPointerDownHandler,IPointerUpHandler
    {
        int optionID = 0;
        bool clicked = false;
        bool pointerOnButton = false;
        Animator animator;
        [SerializeField] AudioClip sound_Click;

        static float WIDTH_NORMAL = 0.701842f;
        static float HEIGH_NORMAL = 0.0924069f;

        void Start()
        {
            if (sound_Click == null)
                sound_Click = Resources.Load("Sound/Button_Click") as AudioClip;

            animator = GetComponent<Animator>();
            animator.enabled = false;

            GetComponent<RectTransform>().sizeDelta = new Vector2(
               WIDTH_NORMAL * BAStoryPlayerController.Instance.StoryPlayer.CanvasRect.sizeDelta.x,
               HEIGH_NORMAL * BAStoryPlayerController.Instance.StoryPlayer.CanvasRect.sizeDelta.y
               );

            transform.localScale = new Vector2(0.95f, 0.95f);
            transform.DoLocalScale(Vector2.one, 0.1f).onComplete = ()=> {
                animator.enabled = true;
            };

            GetComponent<Button>().onClick.AddListener(() =>
            {
                if (clicked) return;
                clicked = true;
                BAStoryPlayerController.Instance.StoryPlayer.AudioModule.Play(sound_Click);
                animator.SetBool("Interactable", false);
                transform.parent.GetComponent<OptionManager>().RevokeInteractablilty(transform);
                EventBus<OnPlayerSelectedBranch>.Raise(new OnPlayerSelectedBranch()
                {
                    scriptGourpID = BAStoryPlayerController.Instance.StoryPlayer.GroupID,
                    selectionGroup = optionID
                });
            });
        }

        public void Initialize(int optionID,string text)
        {
            this.optionID = optionID;
            GetComponentInChildren<TextMeshProUGUI>().text = text;
        }

        public void RunOnComplete()
        {
            transform.parent.GetComponent<OptionManager>().FinishSelecting();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            pointerOnButton = false;

            animator.SetBool("Normal", true);
            animator.SetBool("Pressed", false);
            animator.SetBool("Selected", false);
            animator.SetBool("Highlighted", false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            pointerOnButton = true;

            animator.SetBool("Normal", false);
            animator.SetBool("Pressed", false);
            animator.SetBool("Selected", false);
            animator.SetBool("Highlighted", true);
        }

        public void OnPointerDown(PointerEventData eventData)
        {   
            if(pointerOnButton == true)
            {
                animator.SetBool("Normal", false);
                animator.SetBool("Pressed", true);
                animator.SetBool("Selected", false);
                animator.SetBool("Highlighted", false);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (pointerOnButton == true)
            {
                animator.SetBool("Normal", false);
                animator.SetBool("Pressed", false);
                animator.SetBool("Selected", true);
                animator.SetBool("Highlighted", false);
            }
        }
    }

}
