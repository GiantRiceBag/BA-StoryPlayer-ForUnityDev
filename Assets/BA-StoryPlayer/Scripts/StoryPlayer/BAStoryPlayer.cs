using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using System;
using BAStoryPlayer.DoTweenS;
using BAStoryPlayer.UI;
using Timer = BAStoryPlayer.Utility.Timer;

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
                    onCancelAuto?.Invoke();
                if (auto && isPlaying && isExecutable && !isLocking)
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

        List<StoryUnit> storyUnit;
        [Header("Real-Time Data")]
        [SerializeField] int index_CurrentUnit = 0;
        Queue<int> priorIndex = new Queue<int>(); // �����±���� 
        [SerializeField] bool isPlaying = false;
        [SerializeField] bool isExecutable = true;
        [SerializeField] bool isLocking = false;
        [SerializeField] float lockTime = 0;

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
        public RectTransform CanvasRect => transform.parent.GetComponent<RectTransform>();

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

        [HideInInspector] public UnityEvent<int, int> onUserSelect; // ��һ������Ϊѡ��ID �ڶ�������Ϊ��ID
        [HideInInspector] public UnityEvent onCancelAuto;
        [HideInInspector] public UnityEvent onFinishPlaying;

        Coroutine coroutine_Lock;

        void Start()
        {
            if (image_Background == null)
                image_Background = transform.Find("Background").GetComponent<Image>();

            // �����Լ������¼����� ����һ��ʱ��Ĳ���
            CharacterModule.onAnimateCharacter.AddListener((duration) => { Lock(duration, BAStoryPlayerController.Instance.Setting.Time_Lock_AfterAction); });

            // ѡ���¼�����
            onUserSelect.AddListener((selectionGroup, groupID) =>
            {
                // ����ǰ��Ѱ�������ѡ���±� �����������±���� ����-1��ֹͣ
                for (int i = index_CurrentUnit; i < storyUnit.Count; i++)
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
            UIModule.onFinishPrinting.AddListener(() =>
            {
                Lock(BAStoryPlayerController.Instance.Setting.Time_Lock_AfterPrinting);
            });
        }

        /// <summary>
        /// ���ñ��� ��������Ӧ���
        /// </summary>
        /// <param name="url">���URL</param>
        /// <param name="type">�����л���ʽ �״���Ч</param>
        public void SetBackground(string url = null, TransistionType transition = TransistionType.Instant)
        {
            if (url == null)
            {
                image_Background.sprite = null;
                image_Background.enabled = false;
                return;
            }

            Sprite sprite = Resources.Load<Sprite>(BAStoryPlayerController.Instance.Setting.Path_Background + url);
            Vector2 size = sprite.rect.size;
            float ratio = size.y / size.x;
            size.x = CanvasRect.rect.width;
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
                    default: return;
                }
            }
        }

        /// <summary>
        /// ����ִ�е�Ԫ ���ݳ�ʼ��
        /// </summary>
        public void LoadUnits(int groupID, List<StoryUnit> units)
        {
            priorIndex.Clear();
            this.groupID = groupID;
            storyUnit = units;
            index_CurrentUnit = 0;
            isLocking = false;
            isPlaying = true;
            isExecutable = true;
        }

        /// <summary>
        /// ִ����һ��Ԫ
        /// </summary>
        public void Next(bool breakLock = false)
        {
            // �ı�����
            if (isPlaying && UIModule.IsPrinting && !isExecutable)
            {
                UIModule.Skip();
                if (Auto)
                    Auto = false;
                return;
            }

            // ������ ��Է�Autoģʽ
            if (isLocking && !Auto && !breakLock)
                return;

            if (!isExecutable || !isPlaying)
                return;

            if (index_CurrentUnit == storyUnit.Count)
            {
                CloseStoryPlayer();
                return;
            }

            // ÿһ����Ԫˢ��һ������ʱ��
            ReflashLockTime();

            switch (storyUnit[index_CurrentUnit].type)
            {
                case UnitType.Text:
                case UnitType.Title:
                case UnitType.Option:
                    {
                        storyUnit[index_CurrentUnit].Execute();
                        NextIndex();
                        isExecutable = false;
                        break;
                    }
                case UnitType.Command:
                    {
                        if (storyUnit[index_CurrentUnit].wait != 0)
                        {
                            // ���ݵ�Ԫ�ȴ�ʱ��ִ�еȴ���Ϻ��Զ�ִ����һ��Ԫ
                            storyUnit[index_CurrentUnit].Execute();
                            isExecutable = false;
                            Timer.Delay(() =>
                           {
                               isExecutable = true;
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
                default: return;
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
            isExecutable = true;
            // ��Auto��ֱ��ִ��
            if (next || Auto)
                Next();
        }

        /// <summary>
        /// �رղ�����
        /// </summary>
        /// <param name="fadeOut">�Ƿ�����Ļ������</param>
        /// <param name="destoryObject">�Ƿ�ɾ��������Object</param>
        public void CloseStoryPlayer(bool fadeOut = true, bool destoryObject = false)
        {
            isPlaying = false;
            AudioModule.PauseBGM();

            Timer.Delay(() =>
           {
               if (fadeOut)
               {
                   RequireBackdrop(BackdropType.Out, 1, () =>
                   {
                       onFinishPlaying?.Invoke();
                       EmotionFactory.ClearCache();
                       if (!destoryObject)
                       {
                           gameObject.SetActive(false);
                           SetBackground();
                           UIModule.HideAllUI();
                           CharacterModule.ClearAllObject();
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
                   onFinishPlaying?.Invoke();
                   EmotionFactory.ClearCache();
                   if (!destoryObject)
                   {
                       gameObject.SetActive(false);
                       SetBackground();
                       UIModule.HideAllUI();
                       CharacterModule.ClearAllObject();
                       DoTweenS.DoTweenS.KillAll();
                       AudioModule.ClearAll();
                   }
                   else
                   {
                       Destroy(gameObject);
                   }
               }
           }, 2f);
        }
        public void CloseStoryPlayer() => CloseStoryPlayer(true, false);

        public enum BackdropType
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
        public void RequireBackdrop(BackdropType type,float duration,Action feedback = null)
        {
            GameObject backdrop = new GameObject("Backdrop");
            backdrop.transform.SetParent(transform);
            backdrop.transform.localPosition = Vector3.zero;
            backdrop.transform.localScale = Vector3.one;
            backdrop.AddComponent<RectTransform>().anchorMin = Vector2.zero;
            backdrop.GetComponent<RectTransform>().anchorMax = Vector2.one;
            backdrop.GetComponent<RectTransform>().sizeDelta = backdrop.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            var image = backdrop.AddComponent<Image>();

            switch (type)
            {
                case BackdropType.In:
                    {
                        image.color = new Color(0, 0, 0, 1);
                        image.DoAlpha(0, duration).onComplete = ()=> { feedback?.Invoke(); Destroy(backdrop); };
                        break;
                    }
                case BackdropType.Out:
                    {
                        image.color = new Color(0, 0, 0, 0);
                        image.DoAlpha(1, duration).onComplete = () => { feedback?.Invoke(); Destroy(backdrop); };
                        break;
                    }
                case BackdropType.OutIn:
                    {
                        image.color = new Color(0, 0, 0, 0);
                        image.DoAlpha(1, duration/2).onComplete = () => { feedback?.Invoke(); image.DoAlpha(0, duration/2).onComplete = ()=> { Destroy(backdrop); }; };
                        break;
                    }
                case BackdropType.InOut:
                    {
                        image.color = new Color(0, 0, 0, 1);
                        image.DoAlpha(0, duration/2).onComplete = () => { feedback?.Invoke(); image.DoAlpha(1, duration/2).onComplete = ()=> { Destroy(backdrop); }; };
                        break;
                    }
                default:return;

            }
        }

        /// <summary>
        /// ��������һ��ʱ�� ����ȷ�������������
        /// </summary>
        public void Lock(float duration,float extra = 0.2f)
        {
            if (duration + extra > lockTime)
                ReflashLockTime(duration + extra);
            else
                return;

            if (isLocking)
                StopCoroutine(coroutine_Lock);

            isLocking = true;
            coroutine_Lock = Timer.Delay(transform,() =>
            {
                isLocking = false;  
            }, duration + extra);
        }

        /// <summary>
        /// ˢ�µ�ǰ��Ԫ�����¼�
        /// </summary>
        /// <param name="value"></param>
        public void ReflashLockTime(float value = 0)
        {
            lockTime = value;
        }
    }
}
