using UnityEngine;
using UnityEngine.UI;
using BAStoryPlayer.DoTweenS;
using TMPro;

namespace BAStoryPlayer.UI
{
    public class Button_Option : MonoBehaviour
    {
        int optionID = 0;
        bool clicked = false;

        // Start is called before the first frame update
        void Start()
        {
            GetComponent<Animator>().enabled = false;

            transform.localScale = new Vector2(0.95f, 0.95f);
            transform.DoLocalScale(Vector2.one, 0.1f).onComplete = ()=> {
                GetComponent<Animator>().enabled = true;
            };

            GetComponent<Button>().onClick.AddListener(() =>
            {
                if (clicked) return;
                clicked = true;
                BAStoryPlayerController.Instance.StoryPlayer.AudioManager.Play("Button_Click");
                GetComponent<Animator>().SetBool("Interactable", false);
                transform.parent.GetComponent<OptionManager>().RevokeInteractablilty(transform);
            });
        }

        public void Initialize(int optionID,string text)
        {
            this.optionID = optionID;
            GetComponentInChildren<TextMeshProUGUI>().text = text;
        }

        public void RunOnComplete()
        {
            BAStoryPlayerController.Instance.StoryPlayer.OnPlayerSelect?.Invoke(optionID,BAStoryPlayerController.Instance.StoryPlayer.GroupID);
            Destroy(transform.parent.gameObject);
        }
    }

}
