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
        Queue<int> priorIndex = new Queue<int>(); // �����±���� 
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

        [HideInInspector]public UnityEvent<int,int> OnPlayerSelect; // ��һ������Ϊѡ��ID �ڶ�������Ϊ��ID
        [HideInInspector] public UnityEvent OnCancelAuto;
        [HideInInspector] public UnityEvent OnFinishPlaying;

        Coroutine coroutine_Lock;

        // TEST
        List<StoryUnit> testUnits = new List<StoryUnit>();
        void Start()
        {
            if (image_Backgroup == null)
                image_Backgroup = transform.Find("Backgroup").GetComponent<Image>();

            // �����¼����� ����һ��ʱ��Ĳ���
            CharacterModule.OnAnimateCharacter.AddListener((duration)=> { Lock(duration + 0.5f); });

            // ѡ���¼�����
            OnPlayerSelect.AddListener((selectionGroup,groupID) =>
            {
                // ����ǰ��Ѱ�������ѡ���±� �����������±���� ����-1��ֹͣ
                for(int i = index_CurrentUnit; i < storyUnit.Count; i++)
                {
                    if (storyUnit[i].selectionGroup == selectionGroup)
                    {
                        priorIndex.Enqueue(i);
                    }
                    else if (storyUnit[i].selectionGroup == 0) // ע��������һ����ѡ����ĵ�Ԫ
                    {
                        priorIndex.Enqueue(i);
                        break;
                    }
                }
                // ע�����л��±�
                NextIndex();
            });
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
        /// ����ִ�е�Ԫ ���ݳ�ʼ��
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
        /// ִ����һ��Ԫ
        /// </summary>
        public void Next()
        {
            // ������ ��Է�Autoģʽ
            if (isLocking && !Auto)
                return;

            // �ı�����
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
                // TODO ɾ��������
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
                            // TODO ���ݵ�Ԫ�ȴ�ʱ��ִ�еȴ�
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
        /// ������һ���±�
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
        /// ׼��ִ����һ��Ԫ
        /// </summary>
        /// <param name="next">�Ƿ�ֱ��ִ����һ��Ԫ</param>
        public void ReadyToNext(bool next = false)
        {
            executable = true;
            // ��Auto��ֱ��ִ��
            if (next || Auto)
                Next();
        }

        /// <summary>
        /// �رղ�����
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
                OnFinishPlaying?.Invoke();
            };
        }

        /// <summary>
        /// ��������һ��ʱ�� ����ȷ�������������
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
            UIModule.ShowVenue("�����һ����");
            CharacterModule.ActivateCharacter(2, "shiroko", "00");
            CharacterModule.SetAction(2, CharacterAction.Hophop);
            CharacterModule.SetEmotion(2, CharacterEmotion.Heart);
            UIModule.PrintText("����ҲҪһ��ȥ��������?");
            UIModule.SetSpeaker("shiroko");
        }
    }
}
