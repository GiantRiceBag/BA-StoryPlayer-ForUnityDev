using UnityEngine;
using UnityEngine.UI;

namespace BAStoryPlayer.UI
{
    public class Button_Auto : MonoBehaviour
    {
        [SerializeField] Color color_Selected = Color.red;
        [SerializeField] bool selected = false;
        [SerializeField] AudioClip sound_Click;

        // Start is called before the first frame update
        void Start()
        {
            if (sound_Click == null)
                sound_Click = Resources.Load("Sound/Button_Click") as AudioClip;

            // TODO 临时使用 后期在优化弄的好看点
            GetComponent<Button>().onClick.AddListener(() =>
            {
                selected = !selected;
                BAStoryPlayerController.Instance.StoryPlayer.Auto = selected;
                BAStoryPlayerController.Instance.StoryPlayer.AudioModule.Play(sound_Click);

                if (selected)
                {
                    GetComponent<Image>().color = color_Selected;
                }
                else
                {
                    GetComponent<Image>().color = Color.white;
                }
            });

            BAStoryPlayerController.Instance.StoryPlayer.onCancelAuto.AddListener(() =>
            {
                selected = false;
                GetComponent<Image>().color = Color.white;
            });
        }

        private void OnEnable()
        {
            selected = BAStoryPlayerController.Instance.StoryPlayer.Auto = false;
            GetComponent<Image>().color = Color.white;
        }
    }

}
