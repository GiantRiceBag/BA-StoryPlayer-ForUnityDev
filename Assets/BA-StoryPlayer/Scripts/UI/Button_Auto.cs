using UnityEngine;
using UnityEngine.UI;
using BAStoryPlayer.Event;

namespace BAStoryPlayer.UI
{
    public class Button_Auto : MonoBehaviour
    {
        [SerializeField] private Color color_Selected = new Color(1, 0.8784314f, 0.4235294f);
        [SerializeField] private bool isSelected = false;
        [SerializeField] private AudioClip sound_Click;
        [SerializeField] private GameObject obj_FlowLight;

        public AudioClip Sound_Click
        {
            get
            {
                if (sound_Click == null)
                    sound_Click = Resources.Load("Sound/Button_Click") as AudioClip;
                return sound_Click;
            }
        }
        public GameObject FlowLight 
        {
            get
            {
                if(obj_FlowLight == null)
                    obj_FlowLight = transform.Find("FlowLight").gameObject;
                return obj_FlowLight;
            }
         }

        void Start()
        {
            // TODO 临时使用 后期在优化弄的好看点
            GetComponent<Button>().onClick.AddListener(() =>
            {
                isSelected = !isSelected;
                BAStoryPlayerController.Instance.StoryPlayer.IsAuto = isSelected;
                BAStoryPlayerController.Instance.StoryPlayer.AudioModule.Play(Sound_Click);

                if (isSelected)
                {
                    GetComponent<Image>().color = color_Selected;
                    FlowLight.SetActive(true);
                }
                else
                {
                    GetComponent<Image>().color = Color.white;
                    FlowLight.SetActive(false);
                }
            });

            EventBus<OnPlayerCanceledAuto>.Binding.Add(() =>
            {
                isSelected = false;
                GetComponent<Image>().color = Color.white;
                FlowLight.SetActive(false);
            });
        }

        private void OnEnable()
        {
            isSelected = BAStoryPlayerController.Instance.StoryPlayer.IsAuto = false;
            FlowLight.SetActive(false);
            GetComponent<Image>().color = Color.white;
        }
    }
}
