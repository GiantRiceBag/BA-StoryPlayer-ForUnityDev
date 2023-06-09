using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BAStoryPlayer.UI
{
    public class Button_Menu : MonoBehaviour
    {
        [SerializeField] Color color_Selected_Backgroup;
        [SerializeField] Color color_Selected_Text;
        Color color_Unselected_Text;
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] GameObject subpanel;
        [Space]
        [SerializeField] bool selected = false;
        [SerializeField] AudioClip sound_Click;
        void Start()
        {
            if (sound_Click == null)
                sound_Click = Resources.Load("Sound/Button_Click") as AudioClip;

            if (text == null)
                text = transform.GetComponentInChildren<TextMeshProUGUI>();
            if (subpanel == null)
                subpanel = transform.Find("SubPanel").gameObject;

            subpanel.SetActive(false);
            color_Unselected_Text = text.color;

            GetComponent<Button>().onClick.AddListener(() =>
            {
                BAStoryPlayerController.Instance.StoryPlayer.AudioModule.Play(sound_Click);

                selected = !selected;

                if (selected)
                {
                    text.color = color_Selected_Text;
                    GetComponent<Image>().color = color_Selected_Backgroup;
                }
                else
                {
                    text.color = color_Unselected_Text;
                    GetComponent<Image>().color = Color.white;
                }

                subpanel.SetActive(selected);
            });
        }

        private void OnDisable()
        {
            selected = false;
            subpanel.SetActive(false);
            text.color = color_Unselected_Text;
            GetComponent<Image>().color = Color.white;
        }
    }

}
