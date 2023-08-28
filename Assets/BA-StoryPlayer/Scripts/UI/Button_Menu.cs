using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BAStoryPlayer.DoTweenS;

namespace BAStoryPlayer.UI
{
    public class Button_Menu : MonoBehaviour
    {
        [SerializeField] private Color color_Selected_Background = new Color(0.2f,0.2f,0.2f,1);
        [SerializeField] private Color color_Selected_Text;
        private Color color_Unselected_Text;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private GameObject subpanel;
        [Space]
        [SerializeField] private bool selected = false;
        [SerializeField] private AudioClip sound_Click;

        private Coroutine coroutine_DisableObject;

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

        void SwitchState(bool selected,bool instant = false)
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
                if (instant)
                {
                    image_Subpanel.color = new Color(image_Subpanel.color.r, image_Subpanel.color.g, image_Subpanel.color.b, 0);
                    subpanel.SetActive(false);
                }
                else
                {
                    image_Subpanel.DoAlpha(0, 0.2f).onComplete = () => { subpanel.SetActive(false); };
                }

                foreach (var i in image_SubButton)
                {
                    if (instant)
                        i.color = new Color(i.color.r, i.color.g, i.color.b, 0);
                    else
                        i.DoAlpha(0, 0.1f);
                }
            }
        }

        private void OnDisable()
        {
            if (Application.isEditor)
                return;
            selected = false;
            SwitchState(selected,true);
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
