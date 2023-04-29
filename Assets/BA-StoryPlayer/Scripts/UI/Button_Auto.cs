using UnityEngine;
using UnityEngine.UI;

namespace BAStoryPlayer.UI
{
    public class Button_Auto : MonoBehaviour
    {
        [SerializeField] Color color_Selected = Color.red;
        [SerializeField] bool selected = false;

        // Start is called before the first frame update
        void Start()
        {

            // TODO ��ʱʹ�� �������Ż�Ū�ĺÿ���
            GetComponent<Button>().onClick.AddListener(() =>
            {
                selected = !selected;
                BAStoryPlayerController.Instance.StoryPlayer.Auto = selected;

                if (selected)
                {
                    GetComponent<Image>().color = color_Selected;
                }
                else
                {
                    GetComponent<Image>().color = Color.white;
                }


            });
        }

        private void OnEnable()
        {
            selected = BAStoryPlayerController.Instance.StoryPlayer.Auto = false;
            GetComponent<Image>().color = Color.white;
        }
    }

}
