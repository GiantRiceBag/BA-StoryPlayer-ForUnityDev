using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using BAStoryPlayer.DoTweenS;
using System.Collections.Generic;
using BAStoryPlayer.UI;

namespace BAStoryPlayer
{
    public class BAStoryPlayer : MonoBehaviour
    {
        string PATH_BACKGROUP = "Backgroup/";
        float TIME_TRAINSITION = 1;

        bool auto = false;
        public bool Auto
        {
            set
            {
                auto = value;
                if (!auto)
                    OnCancleAuto?.Invoke();
                if (auto && playing && executable)
                    Next();
            }
            get
            {
                return auto;
            }
        }
        [Space]
        int currentGroupID = -1;

        [Header("References")]
        [SerializeField] Image image_Backgroup;
        [Space]
        [SerializeField] CharacterManager _characterManager;
        [SerializeField] UIManager _UIManager;
        [SerializeField] AudioManager _audioManager;

        [Header("Real-Time Data")]
        List<StoryUnit> storyUnit;
        [SerializeField] int index_Current_Unit = 0;
        [SerializeField] bool playing = false;
        [SerializeField] bool executable = true;

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

        [HideInInspector]public UnityEvent<int,int> OnPlayerSelect; // ��һ������Ϊѡ��ID �ڶ�������Ϊ��ID

        public UnityEvent OnCancleAuto;
        //TODO TEST
        List<StoryUnit> testUnits = new List<StoryUnit>();
        void Start()
        {
            if (image_Backgroup == null)
                image_Backgroup = transform.Find("Backgroup").GetComponent<Image>();
            image_Backgroup.enabled = false;



            StoryUnit unit1 = new StoryUnit();
            StoryUnit unit2 = new StoryUnit();
            StoryUnit unit3 = new StoryUnit();
            StoryUnit unit4 = new StoryUnit();
            StoryUnit unit5 = new StoryUnit();
            StoryUnit unit6 = new StoryUnit();

            unit1.type = UnitType.Title;
            unit1.action += () =>
            {
                SetBackgroup("BG2");
                AudioManager.PlayBGM("Theme_01");
                UIManager.HideAllUI();
                UIManager.ShowTitle("���Ǵ����", "����С����");
            };

            unit2.type = UnitType.Text;
            unit2.action = () =>
            {
                UIManager.ShowVenue("�����һ����");
                CharacterManager.ActivateCharacter(2, "shiroko", "00");
                CharacterManager.SetAction(2, CharacterAction.Hophop);
                CharacterManager.SetEmotion(2, CharacterEmotion.Heart);
                UIManager.PrintText("����ҲҪһ��ȥ��������?");
                UIManager.SetSpeaker("shiroko");
            };

            unit3.type = UnitType.Option;
            unit3.action = () => {
                List<OptionData> dats = new List<OptionData>();
                dats.Add(new OptionData(1, "����Ҫ����һ������?"));
                dats.Add(new OptionData(2, "�Բ��� �Ҿܾ�"));
                UIManager.ShowOption(dats);
            };

            unit4.type = UnitType.Text;
            unit4.action = () =>
            {
                CharacterManager.ActivateCharacter(0, "hoshino", "00");
                CharacterManager.SetAction(0, CharacterAction.AppearL2R,0);
                CharacterManager.SetEmotion(0, CharacterEmotion.Heart);
                UIManager.PrintText("��Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ��!!");
                UIManager.SetSpeaker("hoshino");
            };

            unit5.type = UnitType.Text;
            unit5.action = () =>
            {
                CharacterManager.ActivateCharacter(4, "aru", "06", TransistionType.Smooth);
                CharacterManager.SetAction(4,CharacterAction.Stiff);
                CharacterManager.SetEmotion(4,CharacterEmotion.Sweat);
                UIManager.PrintText("��Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ����Ҳ��!");
                UIManager.SetSpeaker("aru");
            };

            unit6.type = UnitType.Text;
            unit6.action = () =>
            {
                CharacterManager.ActivateCharacter(2, "shiroko", "00");
                CharacterManager.SetAction(2,CharacterAction.Jump);
                CharacterManager.SetEmotion(2,CharacterEmotion.Music);
                UIManager.PrintText("�߰�!");
                UIManager.SetSpeaker("shiroko");
            };


            testUnits.Add(unit1);
            testUnits.Add(unit2);
            testUnits.Add(unit3);
            testUnits.Add(unit4);
            testUnits.Add(unit5);
            testUnits.Add(unit6);
        }

        /// <summary>
        /// ���ñ��� ��������Ӧ���
        /// </summary>
        /// <param name="url">���URL</param>
        /// <param name="type">ƽ���л� �Ա������εǳ���Ч</param>
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

        /// <summary>
        /// ���뵥Ԫ
        /// </summary>
        public void LoadUnits(int groupID,List<StoryUnit> units)
        {
            currentGroupID = groupID;
            storyUnit = units;
            index_Current_Unit = 0;
            playing = true;
        }

        /// <summary>
        /// ִ����һ��Ԫ
        /// </summary>
        public void Next()
        {
            if(index == storyUnit.Count)
            {
                // TODO ɾ��������
                Debug.Log("�������");
                playing = false;
                return;
            }

            if (!executable || !playing)
                return;

            switch (storyUnit[index].type)
            {
                case UnitType.Text:
                case UnitType.Title:
                case UnitType.Option:
                    {
                        storyUnit[index].Execute();
                        index++;
                        executable = false;
                        break;
                    }
                case UnitType.Command:
                    {
                        if(storyUnit[index].wait != 0)
                        {
                            // TODO ���ݵ�Ԫ�ȴ�ʱ��ִ�еȴ�
                        }
                        storyUnit[index].Execute();
                        index++;
                        Next();
                        break;
                    }
                default:return;
            }
        }

        public void ReadyToNext(bool next = false)
        {
            executable = true;
            if (next || Auto)
                Next();
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
            LoadUnits(0, testUnits);
            Next();
        }
        public void TestSpeaker()
        {
            UIManager.ShowVenue("�����һ����");
            CharacterManager.ActivateCharacter(2, "shiroko", "00");
            CharacterManager.SetAction(2, CharacterAction.Hophop);
            CharacterManager.SetEmotion(2, CharacterEmotion.Heart);
            UIManager.PrintText("����ҲҪһ��ȥ��������?");
            UIManager.SetSpeaker("shiroko");
        }
    }
}
