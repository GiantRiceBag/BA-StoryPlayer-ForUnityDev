using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BAStoryPlayer.DoTweenS;

namespace BAStoryPlayer.UI
{
    public class Button_Menu : MonoBehaviour
    {
        [SerializeField] Color color_Selected_Background = new Color(0.2f,0.2f,0.2f,1);
        [SerializeField] Color color_Selected_Text;
        Color color_Unselected_Text;
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] GameObject subpanel;
        [Space]
        [SerializeField] bool selected = false;
        [SerializeField] AudioClip sound_Click;

        Coroutine coroutine_DisableObject;

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
                PlaySound();
                selected = !selected;
                SwitchState(selected);
            });
        }

        void SwitchState(bool selected)
        {
            if (selected)
            {
                text.color = color_Selected_Text;
                GetComponent<Image>().color = color_Selected_Background;

                if (coroutine_DisableObject != null)
                {
                    StopCoroutine(coroutine_DisableObject);
                    coroutine_DisableObject = null;
                }
                coroutine_DisableObject = StartCoroutine(CDisableObject(5));

                subpanel.SetActive(true);
                Image image_Subpanel = subpanel.GetComponent<Image>();
                Image[] image_SubButton = subpanel.GetComponentsInChildren<Image>();

                Color tempCol = image_Subpanel.color;
                tempCol.a = 0;
                image_Subpanel.color = tempCol;
                image_Subpanel.DoAlpha(1, 0.1f).onComplete = () =>
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
                text.color = color_Unselected_Text;
                GetComponent<Image>().color = Color.white;

                if(coroutine_DisableObject != null)
                {
                    StopCoroutine(coroutine_DisableObject);
                    coroutine_DisableObject = null;
                }

                Image image_Subpanel = subpanel.GetComponent<Image>();
                Image[] image_SubButton = subpanel.GetComponentsInChildren<Image>();
                if (image_Subpanel.gameObject.activeSelf)
                {
                    image_Subpanel.DoAlpha(0, 0.2f).onComplete =()=>{ subpanel.SetActive(false); };
                }
                foreach (var i in image_SubButton)
                {
                    if (i.gameObject.activeSelf)
                        i.DoAlpha(0, 0.1f);
                }
            }
        }

        private void OnDisable()
        {
            if (Application.isEditor)
                return;
            selected = false;
            SwitchState(selected);
        }

        public void PlaySound() { BAStoryPlayerController.Instance.StoryPlayer.AudioModule.Play(sound_Click); }

        System.Collections.IEnumerator CDisableObject(float time)
        {
            yield return new WaitForSeconds(time);
            selected = false;
            SwitchState(selected);
        }
    }
}
