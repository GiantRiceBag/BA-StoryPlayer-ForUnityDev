using UnityEngine;
using UnityEngine.UI;

namespace BAStoryPlayer.UI
{
    public class Button_Auto : MonoBehaviour
    {
        [SerializeField] Color color_Selected = new Color(1, 0.8784314f, 0.4235294f);
        [SerializeField] bool selected = false;
        [SerializeField] AudioClip sound_Click;
        [SerializeField] GameObject flowLight;

        void Start()
        {
            if (!sound_Click)
                sound_Click = Resources.Load("Sound/Button_Click") as AudioClip;
            if (!flowLight)
                flowLight = transform.Find("FlowLight").gameObject;

            // TODO 临时使用 后期在优化弄的好看点
            GetComponent<Button>().onClick.AddListener(() =>
            {
                selected = !selected;
                BAStoryPlayerController.Instance.StoryPlayer.Auto = selected;
                BAStoryPlayerController.Instance.StoryPlayer.AudioModule.Play(sound_Click);

                if (selected)
                {
                    GetComponent<Image>().color = color_Selected;
                    flowLight.SetActive(true);
                }
                else
                {
                    GetComponent<Image>().color = Color.white;
                    flowLight.SetActive(false);
                }
            });

            BAStoryPlayerController.Instance.StoryPlayer.onCancelAuto.AddListener(() =>
            {
                selected = false;
                GetComponent<Image>().color = Color.white;
                flowLight.SetActive(false);
            });
        }

        private void OnEnable()
        {
            selected = BAStoryPlayerController.Instance.StoryPlayer.Auto = false;
            GetComponent<Image>().color = Color.white;
        }
    }

}
