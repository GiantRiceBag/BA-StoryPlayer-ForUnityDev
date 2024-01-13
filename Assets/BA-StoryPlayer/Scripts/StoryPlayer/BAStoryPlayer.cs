using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using System;
using BAStoryPlayer.DoTweenS;
using BAStoryPlayer.UI;
using BAStoryPlayer.Event;
using Timer = BAStoryPlayer.Utility.Timer;

namespace BAStoryPlayer
{
    public class BAStoryPlayer : MonoBehaviour
    {
        private bool _isAuto = false;
        [Space]
        private int _groupID = -1;

        [Header("References")]
        [SerializeField] private Image _imgBackground;
        [Space]
        [SerializeField] private CharacterManager _characterModule;
        [SerializeField] private UIManager _uiModule;
        [SerializeField] private AudioManager _audioModule;

        private List<StoryUnit> _storyUnit;
        [Header("Real-Time Data")]
        [SerializeField] private int _currentUnitIndex = 0;
        private Queue<int> _priorUnitIndexes = new Queue<int>(); // �����±���� 
        [SerializeField] private float _lockTime = 0;

        [SerializeField] private bool _isPlaying = false;
        [SerializeField] private bool _isExecutable = true;
        [SerializeField] private bool _isLocking = false;

        private Coroutine _crtLock;

        public bool IsAuto
        {
            set
            {
                _isAuto = value;
                if (!_isAuto)
                {
                    EventBus<OnPlayerCanceledAuto>.Raise();
                    EventBus<OnUnlockedPlayerInput>.ClearCallback();
                }
                if (_isAuto && _isPlaying && _isExecutable) // �ı������Ϻ��״̬
                {
                    if (!IsLocking)
                        Next();
                    else
                        EventBus<OnUnlockedPlayerInput>.AddCallback(() =>
                        {
                            Timer.Delay(() =>
                            {
                                ReadyToNext();
                                EventBus<OnUnlockedPlayerInput>.ClearCallback();
                            }, 1);
                        });
                }
            }
            get
            {
                return _isAuto;
            }
        }
        public bool IsLocking
        {
            set
            {
                _isLocking = value;
                if (_isLocking == false)
                    EventBus<OnUnlockedPlayerInput>.Raise();
            }
            get
            {
                return _isLocking;
            }
        }

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
                if (_uiModule == null)
                    _uiModule = transform.GetComponentInChildren<UIManager>();
                return _uiModule;
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

        public GameObject CanvasObject
        {
            get
            {
                Transform current = transform;
                while(current.parent != null)
                    current = transform.parent;
                return current.gameObject;
            }
        }
        public RectTransform CanvasRect => CanvasObject.GetComponent<RectTransform>();

        public int GroupID => _groupID;
        public float VolumeMusic
        {
            set
            {
                AudioModule.VolumeMusic = value;
            }
            get
            {
                return AudioModule.VolumeMusic;
            }
        }
        public float VolumeSound
        {
            set
            {
                AudioModule.VolumeSound = value;
            }
            get
            {
                return AudioModule.VolumeSound;
            }
        }
        public float VolumeMaster
        {
            set
            {
                AudioModule.VolumeMaster = value;
            }
            get
            {
                return AudioModule.VolumeMaster;
            }
        }

        private void Start()
        {
            if (_imgBackground == null)
                _imgBackground = transform.Find("Background").GetComponent<Image>();

            // �����Լ������¼����� ����һ��ʱ��Ĳ���
            EventBus<OnAnimatedCharacter>.Binding.Add((data) =>
            {
                Lock(data.time, BAStoryPlayerController.Instance.Setting.TimeLockAfterAction);
            });
            // ѡ���¼�����
            EventBus<OnPlayerSelectedBranch>.Binding.Add((data) =>
            {
                // ����ǰ��Ѱ�������ѡ���±� �����������±���� ����0��ֹͣ
                for (int i = _currentUnitIndex; i < _storyUnit.Count; i++)
                {
                    if (_storyUnit[i].selectionGroup == data.selectionGroup)
                    {
                        _priorUnitIndexes.Enqueue(i);
                    }
                    else if (_storyUnit[i].selectionGroup == 0) // ע��������һ����ѡ����ĵ�Ԫ
                    {
                        _priorUnitIndexes.Enqueue(i);
                        break;
                    }
                }
                // ע�����л��±�
                NextIndex();
            });
            // �ı��������ʱ�䶩�� ��������һ��ʱ��
            EventBus<OnPrintedLine>.Binding.Add(() =>
            {
                Lock(BAStoryPlayerController.Instance.Setting.TimeLockAfterPrinting);
            });
        }

        /// <summary>
        /// ���ñ��� ��������Ӧ���
        /// </summary>
        /// <param name="url">���URL</param>
        /// <param name="type">�����л���ʽ �״���Ч</param>
        public void SetBackground(string url = null, BackgroundTransistionType transition = BackgroundTransistionType.Instant)
        {
            if (url == null)
            {
                switch (transition)
                {
                    case BackgroundTransistionType.Instant:
                        _imgBackground.sprite = null;
                        _imgBackground.enabled = false;
                        break;
                    case BackgroundTransistionType.Smooth:
                        _imgBackground.DoColor(Color.black, BAStoryPlayerController.Instance.Setting.TimeSwitchBackground).OnCompleted = () =>
                         {
                             _imgBackground.sprite = null;
                             _imgBackground.enabled = false;
                         };
                        break;
                }
                return;
            }

            Sprite sprite = Resources.Load<Sprite>(BAStoryPlayerController.Instance.Setting.PathBackground + url);

            if (sprite == null)
            {
                Debug.LogError($"û����·�� {BAStoryPlayerController.Instance.Setting.PathBackground + url}  �ҵ����� [{url}]  ");
                return;
            }

            Vector2 size = sprite.rect.size;
            float ratio = size.y / size.x;
            size.x = CanvasRect.rect.width;
            size.y = size.x * ratio;

            _imgBackground.GetComponent<RectTransform>().sizeDelta = size;
            if (!_imgBackground.enabled)
            {
                _imgBackground.enabled = true;
                _imgBackground.sprite = sprite;
                _imgBackground.color = Color.white;
            }
            else
            {
                switch (transition)
                {
                    case BackgroundTransistionType.Instant:
                        {
                            _imgBackground.sprite = sprite;
                            break;
                        }
                    case BackgroundTransistionType.Smooth:
                        {
                            _imgBackground.DoColor(Color.black, BAStoryPlayerController.Instance.Setting.TimeSwitchBackground / 2).OnCompleted = () =>
                            {
                                _imgBackground.sprite = sprite;
                                _imgBackground.DoColor(Color.white, BAStoryPlayerController.Instance.Setting.TimeSwitchBackground / 2);
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
            _priorUnitIndexes.Clear();
            this._groupID = groupID;
            _storyUnit = units;
            _currentUnitIndex = 0;
            IsLocking = false;
            _isPlaying = true;
            _isExecutable = true;
        }

        /// <summary>
        /// ִ����һ��Ԫ
        /// </summary>
        public void Next(bool breakLock = false)
        {
            // �ı�����
            if (_isPlaying && UIModule.IsPrinting && !_isExecutable)
            {
                UIModule.Skip();
                if (IsAuto)
                    IsAuto = false;
                return;
            }

            // ������ ��Է�Autoģʽ
            if (IsLocking && !IsAuto && !breakLock)
                return;

            if (!_isExecutable || !_isPlaying)
                return;

            if (_currentUnitIndex == _storyUnit.Count)
            {
                CloseStoryPlayer();
                return;
            }

            // ÿһ����Ԫˢ��һ������ʱ��
            ReflashLockTime();

            switch (_storyUnit[_currentUnitIndex].type)
            {
                case UnitType.Text:
                case UnitType.Title:
                case UnitType.Option:
                    {
                        _storyUnit[_currentUnitIndex].Execute();
                        NextIndex();
                        _isExecutable = false;
                        break;
                    }
                case UnitType.Command:
                    {
                        if (_storyUnit[_currentUnitIndex].wait != 0)
                        {
                            // ���ݵ�Ԫ�ȴ�ʱ��ִ�еȴ���Ϻ��Զ�ִ����һ��Ԫ
                            _storyUnit[_currentUnitIndex].Execute();
                            _isExecutable = false;
                            Timer.Delay(() =>
                           {
                               _isExecutable = true;
                               NextIndex();
                               Next(true);
                           }, _storyUnit[_currentUnitIndex].wait / 1000f);
                            break;
                        }
                        else
                        {
                            _storyUnit[_currentUnitIndex].Execute();
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
            if (_priorUnitIndexes.Count != 0)
            {
                _currentUnitIndex = _priorUnitIndexes.Dequeue();
            }
            else
                _currentUnitIndex++;
        }

        /// <summary>
        /// ׼��ִ����һ��Ԫ
        /// </summary>
        /// <param name="next">�Ƿ�ֱ��ִ����һ��Ԫ</param>
        public void ReadyToNext(bool next = false)
        {
            _isExecutable = true;
            // ��Auto��ֱ��ִ��
            if (next || IsAuto)
                Next();
        }

        /// <summary>
        /// �رղ�����
        /// </summary>
        /// <param name="fadeOut">�Ƿ�����Ļ������</param>
        /// <param name="destoryObject">�Ƿ�ɾ��������Object</param>
        public void CloseStoryPlayer(bool fadeOut = true, bool destoryObject = false)
        {
            _isPlaying = false;
            AudioModule.PauseBGM();

            Timer.Delay(() =>
           {
               if (fadeOut)
               {
                   RequireBackdrop(BackdropType.Out, 1, () =>
                   {
                       EventBus<OnClosedStoryPlayer>.Raise();
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
                   EventBus<OnClosedStoryPlayer>.Raise();
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
                        image.DoAlpha(0, duration).OnCompleted = ()=> { feedback?.Invoke(); Destroy(backdrop); };
                        break;
                    }
                case BackdropType.Out:
                    {
                        image.color = new Color(0, 0, 0, 0);
                        image.DoAlpha(1, duration).OnCompleted = () => { feedback?.Invoke(); Destroy(backdrop); };
                        break;
                    }
                case BackdropType.OutIn:
                    {
                        image.color = new Color(0, 0, 0, 0);
                        image.DoAlpha(1, duration/2).OnCompleted = () => { feedback?.Invoke(); image.DoAlpha(0, duration/2).OnCompleted = ()=> { Destroy(backdrop); }; };
                        break;
                    }
                case BackdropType.InOut:
                    {
                        image.color = new Color(0, 0, 0, 1);
                        image.DoAlpha(0, duration/2).OnCompleted = () => { feedback?.Invoke(); image.DoAlpha(1, duration/2).OnCompleted = ()=> { Destroy(backdrop); }; };
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
            if (duration + extra > _lockTime)
                ReflashLockTime(duration + extra);
            else
                return;

            if (IsLocking)
                StopCoroutine(_crtLock);

            IsLocking = true;
            _crtLock = Timer.Delay(transform,() =>
            {
                IsLocking = false;
            }, duration + extra);
        }

        /// <summary>
        /// ˢ�µ�ǰ��Ԫ�����¼�
        /// </summary>
        /// <param name="value"></param>
        public void ReflashLockTime(float value = 0)
        {
            _lockTime = value;
        }
    }
}
