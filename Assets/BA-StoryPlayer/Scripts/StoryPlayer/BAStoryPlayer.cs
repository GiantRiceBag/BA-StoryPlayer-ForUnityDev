using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using System;
using BAStoryPlayer.DoTweenS;
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
                    OnCancelAuto?.Invoke();
                if (auto && isPlaying && executable)
                    Next();
            }
            get
            {
                return auto;
            }
        }
        [Space]
        int groupID = -1;

        [Header("References")]
        [SerializeField] Image image_Backgroup;
        [Space]
        [SerializeField] CharacterManager _characterModule;
        [SerializeField] UIManager _UIModule;
        [SerializeField] AudioManager _audioModule;

        [Header("Real-Time Data")]
        List<StoryUnit> storyUnit;
        [SerializeField] int index_CurrentUnit = 0;
        Queue<int> priorIndex = new Queue<int>(); // 优先下标队列 
        [SerializeField] bool isPlaying = false;
        [SerializeField] bool executable = true;
        [SerializeField] bool isLocking = false;

        public CharacterManager CharacterModule
        {
            get
            {
                if (_characterModule == null)
                    _characterModule = transform.GetComponentInChildren<CharacterManager>();
                return _characterModule;
            }
        }
        public UIManager UIModule
        {
            get
            {
                if (_UIModule == null)
                    _UIModule = transform.GetComponentInChildren<UIManager>();
                return _UIModule;
            }
        }
        public AudioManager AudioModule
        {
            get
            {
                if (_audioModule == null)
                    _audioModule = transform.GetComponent<AudioManager>();
                if (_audioModule == null)
                    _audioModule = gameObject.AddComponent<AudioManager>();
                return _audioModule;
            }
        }

        public int GroupID
        {
            get
            {
                return groupID;
            }
        }
        public float Volume_Music
        {
            set
            {
                AudioModule.Volume_Music = value;
            }
            get
            {
                return AudioModule.Volume_Music;
            }
        }
        public float Volume_Sound
        {
            set
            {
                AudioModule.Volume_Sound = value;
            }
            get
            {
                return AudioModule.Volume_Sound;
            }
        }
        public float Volume_Master
        {
            set
            {
                AudioModule.Volume_Master = value;
            }
            get
            {
                return AudioModule.Volume_Master;
            }
        }

        [HideInInspector]public UnityEvent<int,int> OnPlayerSelect; // 第一个参数为选项ID 第二个参数为组ID
        [HideInInspector] public UnityEvent OnCancelAuto;

        Coroutine coroutine_Lock;

        // TEST
        List<StoryUnit> testUnits = new List<StoryUnit>();
        void Start()
        {
            if (image_Backgroup == null)
                image_Backgroup = transform.Find("Backgroup").GetComponent<Image>();
            image_Backgroup.enabled = false;

            CharacterModule.OnAnimateCharacter.AddListener((duration)=> { Lock(duration); });

            // 选项事件订阅
            OnPlayerSelect.AddListener((selectionID,groupID) =>
            {
                // 坐标前移寻找最近的选项下标 并放入优先下标队列 遇到-1则停止
                for(int i = index_CurrentUnit; i < storyUnit.Count; i++)
                {
                    if (storyUnit[i].selectionGroup == selectionID)
                    {
                        priorIndex.Enqueue(i);
                    }
                    else if (storyUnit[i].selectionGroup == -1) // 注意添加最后一个无选项组的单元
                    {
                        priorIndex.Enqueue(i);
                        break;
                    }
                }
                // 注意先切换下标
                NextIndex();
            });

            // TEST
            StoryUnit unit1 = new StoryUnit();
            StoryUnit unit2 = new StoryUnit();
            StoryUnit unit3 = new StoryUnit();
            StoryUnit unit4 = new StoryUnit();
            StoryUnit unit5 = new StoryUnit();
            StoryUnit unit6 = new StoryUnit();
            StoryUnit unit31 = new StoryUnit();
            StoryUnit unit32 = new StoryUnit();
            StoryUnit unit321 = new StoryUnit();

            unit1.type = UnitType.Title;
            unit1.action += () =>
            {
                SetBackgroup("BG2");
                AudioModule.PlayBGM("Theme_01");
                UIModule.ShowTitle("我是大标题", "我是小标题");
            };

            unit2.type = UnitType.Text;
            unit2.action = () =>
            {
                UIModule.ShowVenue("世界第一银行：行动前准备");
                CharacterModule.ActivateCharacter(2, "shiroko", "00", "今天也要一起去抢银行吗?");
                CharacterModule.SetAction(2, CharacterAction.Hophop);
                CharacterModule.SetEmotion(2, CharacterEmotion.Heart);
            };

            unit3.type = UnitType.Option;
            unit3.action = () => {
                List<OptionData> dats = new List<OptionData>();
                dats.Add(new OptionData(1, "今天要抢哪一个银行?"));
                dats.Add(new OptionData(2, "对不起 我拒绝"));
                UIModule.ShowOption(dats);
            };

            unit4.type = UnitType.Text;
            unit4.action = () =>
            {
                CharacterModule.ActivateCharacter(0, "hoshino", "00", "我也来我也来我也来我也来我也来我也来我也来我也来我也来我也来我也来我也来我也来我也来我也来我也来我也来我也来我也来我也来我也来我也来!!");
                CharacterModule.SetAction(0, CharacterAction.AppearL2R,0);
                CharacterModule.SetEmotion(0, CharacterEmotion.Twinkle);
            };

            unit5.type = UnitType.Text;
            unit5.action = () =>
            {
                CharacterModule.ActivateCharacter(4, "aru", "06", "你们认真的吗!?", TransistionType.Smooth);
                CharacterModule.SetAction(4,CharacterAction.Stiff);
                CharacterModule.SetEmotion(4,CharacterEmotion.Surprise);
            };

            unit6.type = UnitType.Text;
            unit6.action = () =>
            {
                CharacterModule.ActivateCharacter(2, "shiroko", "00", "走吧!");
                CharacterModule.SetAction(2,CharacterAction.Jump);
                CharacterModule.SetEmotion(2,CharacterEmotion.Music);
            };

            unit31.type = UnitType.Text;
            unit31.selectionGroup = 1;
            unit31.action = () =>
            {
                CharacterModule.ActivateCharacter(2, "shiroko", "00", "什么银行随你挑。");
                CharacterModule.SetAction(2,CharacterAction.Greeting);
                CharacterModule.SetEmotion(2,CharacterEmotion.Chat);
            };
            unit32.type = UnitType.Text;
            unit32.selectionGroup = 2;
            unit32.action = () =>
            {
                CharacterModule.ActivateCharacter(2, "shiroko", "05", "!?");
                CharacterModule.SetAction(2, CharacterAction.Stiff);
                CharacterModule.SetEmotion(2, CharacterEmotion.Surprise);
            };
            unit321.type = UnitType.Text;
            unit321.selectionGroup = 2;
            unit321.action = () =>
            {
                CharacterModule.ActivateCharacter(2, "shiroko", "06", "你可没有拒绝的权力。");
                CharacterModule.SetEmotion(2, CharacterEmotion.Angry);
            };


            testUnits.Add(unit1);
            testUnits.Add(unit2);
            testUnits.Add(unit3);
            testUnits.Add(unit31);
            testUnits.Add(unit32);
            testUnits.Add(unit321);
            testUnits.Add(unit4);
            testUnits.Add(unit5);
            testUnits.Add(unit6);
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

        /// <summary>
        /// 载入执行单元 数据初始化
        /// </summary>
        public void LoadUnits(int groupID,List<StoryUnit> units)
        {
            priorIndex.Clear();
            this.groupID = groupID;
            storyUnit = units;
            index_CurrentUnit = 0;
            isLocking = false;
            isPlaying = true;
            executable = true;
        }

        /// <summary>
        /// 执行下一单元
        /// </summary>
        public void Next()
        {
            // 操作锁 针对非Auto模式
            if (isLocking && !Auto)
                return;

            // 文本跳过
            if(isPlaying && UIModule.IsPriting && !executable)
            {
                UIModule.Skip();
                if (Auto)
                    Auto = false;
                return;
            }

            if (!executable || !isPlaying)
                return;

            if (index_CurrentUnit == storyUnit.Count)
            {
                // TODO 删除播放器
                Debug.Log("播放完成");
                isPlaying = false;
                CloseStoryPlayer();
                return;
            }

            switch (storyUnit[index_CurrentUnit].type)
            {
                case UnitType.Text:
                case UnitType.Title:
                case UnitType.Option:
                    {
                        storyUnit[index_CurrentUnit].Execute();
                        NextIndex();
                        executable = false;
                        break;
                    }
                case UnitType.Command:
                    {
                        if(storyUnit[index_CurrentUnit].wait != 0)
                        {
                            // TODO 根据单元等待时间执行等待
                        }
                        storyUnit[index_CurrentUnit].Execute();
                        NextIndex();
                        Next();
                        break;
                    }
                default:return;
            }
        }

        /// <summary>
        /// 加载下一个下标
        /// </summary>
        void NextIndex()
        {
            if (priorIndex.Count != 0)
            {
                index_CurrentUnit = priorIndex.Dequeue();
            }
            else
                index_CurrentUnit++;
        }

        /// <summary>
        /// 准备执行下一单元
        /// </summary>
        /// <param name="next">是否直接执行下一单元</param>
        public void ReadyToNext(bool next = false)
        {
            executable = true;
            // 若Auto则直接执行
            if (next || Auto)
                Next();
        }

        /// <summary>
        /// 关闭播放器
        /// </summary>
        void CloseStoryPlayer()
        {
            AudioModule.PauseBGM();
            GameObject curtain = Instantiate(new GameObject("Curtain"));
            curtain.transform.SetParent(transform);
            curtain.transform.localPosition = Vector3.zero;
            curtain.transform.localScale = Vector3.one;
            curtain.AddComponent<RectTransform>().sizeDelta = GetComponent<RectTransform>().sizeDelta;
            var image = curtain.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0);
            image.DoAlpha(1, 1f).onComplete = ()=> {
                Destroy(gameObject);
            };
        }

        /// <summary>
        /// 锁定操作一段时间 用于确保动作播放完毕
        /// </summary>
        public void Lock(float duration,float extra = 0.5f)
        {
            if (isLocking)
                StopCoroutine(coroutine_Lock);

            isLocking = true;
            coroutine_Lock = Delay(transform,() =>
            {
                isLocking = false;
            }, duration + extra);
        }

        public static Coroutine Delay(Transform obj, Action action, float duration)
        {
            return obj.GetComponent<MonoBehaviour>().StartCoroutine(CDelay(action, duration));
        }
        static System.Collections.IEnumerator CDelay(Action action, float duration)
        {
            yield return new WaitForSeconds(duration);
            action?.Invoke();
        }

        // TEST
        int indexTest = 0;
        public void TestBG()
        {
            SetBackgroup($"BG{indexTest+1}", TransistionType.Smooth);
            indexTest++;
            indexTest %= 5;
        }
        public void TestAll()
        {
            LoadUnits(0, testUnits);
            Next();
        }
        public void TestSpeaker()
        {
            UIModule.ShowVenue("世界第一银行");
            CharacterModule.ActivateCharacter(2, "shiroko", "00");
            CharacterModule.SetAction(2, CharacterAction.Hophop);
            CharacterModule.SetEmotion(2, CharacterEmotion.Heart);
            UIModule.PrintText("今天也要一起去抢银行吗?");
            UIModule.SetSpeaker("shiroko");
        }
    }
}
