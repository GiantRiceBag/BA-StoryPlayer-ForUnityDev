using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using BAStoryPlayer.DoTweenS;
using TMPro;

namespace BAStoryPlayer.UI
{
    public class Button_Option : MonoBehaviour,IPointerExitHandler,IPointerEnterHandler,IPointerDownHandler,IPointerUpHandler
    {
        int optionID = 0;
        bool clicked = false;
        bool pointerOnButton = false;
        Animator animator;

        // Start is called before the first frame update
        void Start()
        {
            animator = GetComponent<Animator>();
            animator.enabled = false;

            transform.localScale = new Vector2(0.95f, 0.95f);
            transform.DoLocalScale(Vector2.one, 0.1f).onComplete = ()=> {
                animator.enabled = true;
            };

            GetComponent<Button>().onClick.AddListener(() =>
            {
                if (clicked) return;
                clicked = true;
                BAStoryPlayerController.Instance.StoryPlayer.AudioModule.Play("Button_Click");
                animator.SetBool("Interactable", false);
                transform.parent.GetComponent<OptionManager>().RevokeInteractablilty(transform);
                BAStoryPlayerController.Instance.StoryPlayer.OnPlayerSelect?.Invoke(optionID, BAStoryPlayerController.Instance.StoryPlayer.GroupID);
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
