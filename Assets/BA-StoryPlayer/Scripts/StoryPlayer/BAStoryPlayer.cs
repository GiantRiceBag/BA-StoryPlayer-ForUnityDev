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
        [SerializeField] Image image_Background;
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

        [HideInInspector]public UnityEvent<int,int> OnUserSelect; // ��һ������Ϊѡ��ID �ڶ�������Ϊ��ID
        [HideInInspector] public UnityEvent OnCancelAuto;
        [HideInInspector] public UnityEvent OnFinishPlaying;

        Coroutine coroutine_Lock;

        void Start()
        {
            if (image_Background == null)
                image_Background = transform.Find("Background").GetComponent<Image>();

            // �����¼����� ����һ��ʱ��Ĳ���
            CharacterModule.OnAnimateCharacter.AddListener((duration)=> { Lock(duration + BAStoryPlayerController.Instance.Setting.Time_Lock_AfterAction); });

            // ѡ���¼�����
            OnUserSelect.AddListener((selectionGroup,groupID) =>
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

            // �ı��������ʱ�䶩�� ��������һ��ʱ��
            UIModule.OnFinishPrinting.AddListener(() =>
            {
                Lock(BAStoryPlayerController.Instance.Setting.Time_Lock_AfterPrinting);
            });
        }

        /// <summary>
        /// ���ñ��� ��������Ӧ���
        /// </summary>
        /// <param name="url">���URL</param>
        /// <param name="type">ƽ���л� �Ա������εǳ���Ч</param>
        public void SetBackground(string url = null,TransistionType transition = TransistionType.Instant)
        {
            if(url == null)
            {
                image_Background.sprite = null;
                image_Background.enabled = false;
                return;
            }

            Sprite sprite = Resources.Load<Sprite>(BAStoryPlayerController.Instance.Setting.Path_Background + url);
            Vector2 size = sprite.rect.size;
            float ratio = size.y / size.x;
            size.x = 1920;
            size.y = size.x * ratio;

            image_Background.GetComponent<RectTransform>().sizeDelta = size;
            if (!image_Background.enabled)
            {
                image_Background.enabled = true;
                image_Background.sprite = sprite;
            }
            else
            {
                switch (transition)
                {
                    case TransistionType.Instant:
                        {
                            image_Background.sprite = sprite;
                            break;
                        }
                    case TransistionType.Smooth:
                        {
                            image_Background.DoColor(Color.black, BAStoryPlayerController.Instance.Setting.Time_SwitchBAckground / 2).onComplete = () =>
                            {
                                image_Background.sprite = sprite;
                                image_Background.DoColor(Color.white, BAStoryPlayerController.Instance.Setting.Time_SwitchBAckground / 2);
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
        public void Next(bool breakLock = false)
        {
            // ������ ��Է�Autoģʽ
            if (isLocking && !Auto && !breakLock)
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
                            // ���ݵ�Ԫ�ȴ�ʱ��ִ�еȴ���Ϻ��Զ�ִ����һ��Ԫ
                            storyUnit[index_CurrentUnit].Execute();
                            executable = false;
                            Delay(transform, () =>
                            {
                                executable = true;
                                NextIndex();
                                Next(true);
                            }, storyUnit[index_CurrentUnit].wait / 1000f);
                            break;
                        }
                        else
                        {
                            storyUnit[index_CurrentUnit].Execute();
                            NextIndex();
                            Next(true);
                            break;
                        }

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
        /// <param name="fadeOut">�Ƿ�����Ļ������</param>
        /// <param name="destoryObject">�Ƿ�ɾ��������Object</param>
        void CloseStoryPlayer(bool fadeOut = true,bool destoryObject = false)
        {
            isPlaying = false;
            AudioModule.PauseBGM();

            Delay(transform, () =>
            {
                if (fadeOut)
                {
                    CreateCurtain(CurtainType.Out, 1, () =>
                    {
                        OnFinishPlaying?.Invoke();

                        if (!destoryObject)
                        {
                            gameObject.SetActive(false);
                            SetBackground();
                            UIModule.HideAllUI();
                            CharacterModule.ClearAll();
                            DoTweenS.DoTweenS.KillAll();
                            AudioModule.ClearAll();
                        }
                        else
                        {
                            Destroy(gameObject);
                        }
                    });
                }
                else
                {
                    if (!destoryObject)
                    {
                        gameObject.SetActive(false);
                        SetBackground();
                        UIModule.HideAllUI();
                        CharacterModule.ClearAll();
                        DoTweenS.DoTweenS.KillAll();
                        AudioModule.ClearAll();
                    }
                    else
                    {
                        Destroy(gameObject);
                    }
                }


            }, 1f);
        }

        public enum CurtainType
        {
            In,
            Out,
            OutIn,
            InOut
        }
        /// <summary>
        /// ����Ļ��
        /// </summary>
        /// <param name="type">Ļ������</param>
        /// <param name="duration">����ʱ��</param>
        /// <param name="feedback">�ص�����</param>
        public void CreateCurtain(CurtainType type,float duration,Action feedback = null)
        {
            GameObject curtain = new GameObject("Curtain");
            curtain.transform.SetParent(transform);
            curtain.transform.localPosition = Vector3.zero;
            curtain.transform.localScale = Vector3.one;
            curtain.AddComponent<RectTransform>().sizeDelta = GetComponent<RectTransform>().sizeDelta;
            var image = curtain.AddComponent<Image>();

            switch (type)
            {
                case CurtainType.In:
                    {
                        image.color = new Color(0, 0, 0, 1);
                        image.DoAlpha(0, duration).onComplete = ()=> { feedback?.Invoke(); Destroy(curtain); };
                        break;
                    }
                case CurtainType.Out:
                    {
                        image.color = new Color(0, 0, 0, 0);
                        image.DoAlpha(1, duration).onComplete = () => { feedback?.Invoke(); Destroy(curtain); };
                        break;
                    }
                case CurtainType.OutIn:
                    {
                        image.color = new Color(0, 0, 0, 0);
                        image.DoAlpha(1, duration/2).onComplete = () => { feedback?.Invoke(); image.DoAlpha(0, duration/2).onComplete = ()=> { Destroy(curtain); }; };
                        break;
                    }
                case CurtainType.InOut:
                    {
                        image.color = new Color(0, 0, 0, 1);
                        image.DoAlpha(0, duration/2).onComplete = () => { feedback?.Invoke(); image.DoAlpha(1, duration/2).onComplete = ()=> { Destroy(curtain); }; };
                        break;
                    }
                default:return;

            }
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

        /// <summary>
        /// �ӳ�ί��
        /// </summary>
        public static Coroutine Delay(Transform obj, Action action, float duration)
        {
            return obj.GetComponent<MonoBehaviour>().StartCoroutine(CDelay(action, duration));
        }
        static System.Collections.IEnumerator CDelay(Action action, float duration)
        {
            yield return new WaitForSeconds(duration);
            action?.Invoke();
        }
    }
}
