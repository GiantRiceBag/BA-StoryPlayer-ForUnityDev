using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using BAStoryPlayer.DoTweenS;

namespace BAStoryPlayer
{
    public class BAStoryPlayer : MonoBehaviour
    {
        string PATH_BACKGROUP = "Backgroup/";
        float TIME_TRAINSITION = 1;

        public bool Auto;
        [Space]
        int currentGroupID = -1;

        [Header("References")]
        [SerializeField] Image image_Backgroup;
        [Space]
        [SerializeField] CharacterManager _characterManager;
        [SerializeField] UIManager _UIManager;
        [SerializeField] AudioManager _audioManager;

        public CharacterManager CharacterManager
        {
            get
            {
                if (_characterManager == null)
                    _characterManager = transform.GetComponentInChildren<CharacterManager>();
                return _characterManager;
            }
        }
        public UIManager UIManager
        {
            get
            {
                if (_UIManager == null)
                    _UIManager = transform.GetComponentInChildren<UIManager>();
                return _UIManager;
            }
        }
        public AudioManager AudioManager
        {
            get
            {
                if (_audioManager == null)
                    _audioManager = transform.GetComponent<AudioManager>();
                if (_audioManager == null)
                    _audioManager = gameObject.AddComponent<AudioManager>();
                return _audioManager;
            }
        }

        public int GroupID
        {
            get
            {
                return currentGroupID;
            }
        }
        public float Volume_Music
        {
            set
            {
                AudioManager.Volume_Music = value;
            }
            get
            {
                return AudioManager.Volume_Music;
            }
        }
        public float Volume_Sound
        {
            set
            {
                AudioManager.Volume_Sound = value;
            }
            get
            {
                return AudioManager.Volume_Sound;
            }
        }
        public float Volume_Master
        {
            set
            {
                AudioManager.Volume_Master = value;
            }
            get
            {
                return AudioManager.Volume_Master;
            }
        }

        [HideInInspector]public UnityEvent<int,int> OnPlayerSelect; // 第一个参数为选项ID 第二个参数为组ID

        void Start()
        {
            if (image_Backgroup == null)
                image_Backgroup = transform.Find("Backgroup").GetComponent<Image>();
            image_Backgroup.enabled = false;

            // TODO Test
            //CharacterManager.ActivateCharacter(0, "hoshino", "00", TransistionType.Smooth);
            //CharacterManager.ActivateCharacter(1, "aru", "00", TransistionType.Smooth);
            //CharacterManager.ActivateCharacter(2, "serika_shibaseki", "00");
            //CharacterManager.ActivateCharacter(3, "shiroko", "00", TransistionType.Smooth);
            //CharacterManager.ActivateCharacter(4, "kayoko", "00", TransistionType.Smooth);

        }

        /// <summary>
        /// 设置背景 并优先适应宽度
        /// </summary>
        /// <param name="url">相对URL</param>
        /// <param name="type">平缓切换 对背景初次登场无效</param>
        public void SetBackgroup(string url,TransistionType transition = TransistionType.Instant)
        {
            Sprite sprite = Resources.Load<Sprite>(PATH_BACKGROUP + url);
            Vector2 size = sprite.rect.size;
            float ratio = size.y / size.x;
            size.x = 1920;
            size.y = size.x * ratio;

            image_Backgroup.GetComponent<RectTransform>().sizeDelta = size;

            if (!image_Backgroup.enabled)
            {
                image_Backgroup.enabled = true;
                image_Backgroup.sprite = sprite;
            }
            else
            {
                switch (transition)
                {
                    case TransistionType.Instant:
                        {
                            image_Backgroup.sprite = sprite;
                            break;
                        }
                    case TransistionType.Smooth:
                        {
                            image_Backgroup.DoColor(Color.black, TIME_TRAINSITION / 2).onComplete = () =>
                            {
                                image_Backgroup.sprite = sprite;
                                image_Backgroup.DoColor(Color.white, TIME_TRAINSITION / 2);
                            };
                            break;
                        }
                    default:return;
                }
            }


        }

        // TODO TEST
        int index = 0;
        public void TestBG()
        {
            SetBackgroup($"BG{index+1}", TransistionType.Smooth);
            index++;
            index %= 5;
        }
        public void TestAll()
        {
            SetBackgroup("BG2");
            AudioManager.PlayBGM("Theme_01");
            UIManager.HideAllUI();
            UIManager.ShowTitle("我是大标题", "我是小标题");
        }
        public void TestSpeaker()
        {
            UIManager.ShowVenue("世界第一银行");
            CharacterManager.ActivateCharacter(2, "shiroko", "00");
            CharacterManager.SetAction(2, CharacterAction.Hophop);
            CharacterManager.SetEmotion(2, CharacterEmotion.Heart);
            UIManager.PrintText("今天也要一起去抢银行吗?");
            UIManager.SetSpeaker("shiroko");
        }
    }
}
