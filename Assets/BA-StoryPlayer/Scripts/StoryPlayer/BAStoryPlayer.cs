using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections;
using System;

using BAStoryPlayer.DoTweenS;
using BAStoryPlayer.UI;
using BAStoryPlayer.Event;
using BAStoryPlayer.Parser.UniversaScriptParser;

namespace BAStoryPlayer
{
    public class BAStoryPlayer : MonoBehaviour
    {
        private bool _isAuto = false;
        [Space]
        private int _groupID = -1;

        [Header("References")]
        [SerializeField] private BackgroundManager _backgroundModule;
        [SerializeField] private CharacterManager _characterModule;
        [SerializeField] private UIManager _uiModule;
        [SerializeField] private AudioManager _audioModule;
        [Space]
        [SerializeField] private CharacterDataTable _characterDataTable;
        [SerializeField] private PlayerSetting _playerSetting;

        private List<StoryUnit> _storyUnits;
        private StoryUnit _currentStoryUnit;

        [Header("Real-Time Data")]
        private int _currentUnitIndex = 0;
        private Queue<StoryUnit> _priorStoryUnits = new(); // ���ȵ�Ԫ
        [SerializeField] private float _lockTime = 0;

        [SerializeField] private bool _isPlaying = false;
        [SerializeField] private bool _isExecutable = true;
        [SerializeField] private bool _isLocking = false;
        private bool _isSkippable = true;

        private Coroutine _crtLock;

        public StoryUnit CurrentStoryUnit => _currentStoryUnit;
        public int CurrentUnitIndex => _currentUnitIndex;
        public int UnitCount => _storyUnits.Count;
        public bool IsBranchUnit =>CurrentStoryUnit != null && !_storyUnits.Contains(CurrentStoryUnit);

        public bool IsPlaying
        {
            get => _isPlaying; 
            private set => _isPlaying = value;
        }
        public bool IsAuto
        {
            set
            {
                _isAuto = value;
                if (!_isAuto)
                {
                    EventBus<OnCanceledAuto>.Raise();
                    EventBus<OnUnlockedPlayerInput>.ClearCallback();
                }
                if (_isAuto && _isPlaying && IsExecutable) // �ı������Ϻ��״̬
                {
                    if (!IsLocking)
                        ExecuteCurrentUnit();
                    else
                        EventBus<OnUnlockedPlayerInput>.AddCallback(() =>
                        {
                            this.Delay(() =>
                            {
                                ReadyToExecute();
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
        public bool IsExecutable
        {
            get => _isExecutable;
            private set => _isExecutable = value;
        }
        public bool IsSkippable
        {
            set => _isSkippable = value;
            get => _isSkippable;
        }
        public bool IsReadyToClosePlayer
        {
            get
            {
                if(IsPlaying && _currentUnitIndex == _storyUnits.Count)
                {
                    return true;
                }

                return false;
            }
        }

        public BackgroundManager BackgroundModule
        {
            get
            {
                if (_backgroundModule == null)
                {
                    _backgroundModule = transform.GetComponentInChildren<BackgroundManager>();
                }
                return _backgroundModule;
            }
        }
        public CharacterManager CharacterModule
        {
            get
            {
                if (_characterModule == null)
                {
                    _characterModule = transform.GetComponentInChildren<CharacterManager>();
                }
                return _characterModule;
            }
        }
        public UIManager UIModule
        {
            get
            {
                if (_uiModule == null)
                {
                    _uiModule = transform.GetComponentInChildren<UIManager>();
                }
                return _uiModule;
            }
        }
        public AudioManager AudioModule
        {
            get
            {
                if (_audioModule == null)
                {
                    _audioModule = transform.GetComponent<AudioManager>();
                }
                if (_audioModule == null)
                {
                    _audioModule = gameObject.AddComponent<AudioManager>();
                }
                return _audioModule;
            }
        }

        public CharacterDataTable CharacterDataTable
        {
            get
            {
                if (_characterDataTable == null)
                {
                    _characterDataTable = TryGetCharactetDatable();
                    if(_characterDataTable == null)
                    {
                        Debug.LogError($"δ���ý�ɫ��Ϣ��!");
                    }
                }

                return _characterDataTable;
            }
        }
        public PlayerSetting Setting
        {
            get
            {
                if (_playerSetting == null)
                {
                    _playerSetting = TryGetPlayerSetting();
                }

                return _playerSetting;
            }
        }

        public GameObject CanvasObject
        {
            get
            {
                Transform current = transform;
                while(current != null)
                {
                    if(current.TryGetComponent(out Canvas canvas))
                    {
                        if(canvas != null)
                        {
                            return canvas.gameObject;
                        }
                    }
                    current = current.parent;
                }
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

        public Dictionary<string, int> FlagTable { get; set; }
        public Dictionary<string, int> ModifiedFlagTable { get; set; }
        public List<string> ScriptsToExecute { get; set; }

        public event Action<List<string>, Dictionary<string, int>> OnStoryPlayerClosed;

        private void Awake()
        {
            FlagTable = new();
            ModifiedFlagTable = new();
            ScriptsToExecute = new();
            
            if(_playerSetting != null)
            {
                _playerSetting = ScriptableObject.Instantiate(_playerSetting);
            }
            if(_characterDataTable != null)
            {
                _characterDataTable = ScriptableObject.Instantiate(_characterDataTable);
            }
        }

        private void OnEnable()
        {
            EventBus<OnSetCharacterAction>.Binding.Add(OnSetCharacterActionEventHandler);
            EventBus<OnPlayerSelected>.Binding.Add(OnPlayerSelectedEventHandler);
            EventBus<OnFinishedPrintingMainText>.Binding.Add(OnFinishedPrintingMainTextEventHandler);
        }
        private void OnDisable()
        {
            EventBus<OnSetCharacterAction>.Binding.Remove(OnSetCharacterActionEventHandler);
            EventBus<OnPlayerSelected>.Binding.Remove(OnPlayerSelectedEventHandler);
            EventBus<OnFinishedPrintingMainText>.Binding.Remove(OnFinishedPrintingMainTextEventHandler);
        }

        public bool LoadStory(string scriptName,Dictionary<string,int> flagTable = null)
        {
            var textAsset = Resources.Load<TextAsset>(Setting.PathStoryScript + scriptName);
            if (textAsset == null)
            {
                Debug.LogError($"δ���� {Setting.PathStoryScript + scriptName} �ҵ�����ű�");
                return false;
            }

            UniversalStoryScript storyScript = JsonUtility.FromJson<UniversalStoryScript>(textAsset.text);
            return LoadStory(storyScript,flagTable);
        }
        public bool LoadStory(UniversalStoryScript storyScript, Dictionary<string, int> flagTable = null)
        {
            if (IsPlaying)
            {
                Debug.Log("���鲥����");
                return false;
            }

            UniversalCommandParser parser = new UniversalCommandParser(this);
            List<StoryUnit> units = parser.Parse(storyScript);

            if(units.Count == 0)
            {
                Debug.Log("�޿�ִ�е�Ԫ");
                return false;
            }

            FlagTable = flagTable;
            ModifiedFlagTable?.Clear();
            ScriptsToExecute?.Clear();

            IsSkippable = true;
            IsPlaying = true;
            gameObject.SetActive(true);

            LoadUnits(0, units);
            ReadyToExecute();
            ExecuteCurrentUnit();

            EventBus<OnClosedStoryPlayer>.ClearCallback();
            EventBus<OnClosedStoryPlayer>.AddCallback(() =>
            {
                _isPlaying = false;
            });
            EventBus<OnStartPlayingStory>.Raise();

            return true;
        }

        private void LoadUnits(int groupID, List<StoryUnit> units)
        {
            if(units.Count == 0)
            {
                CloseStoryPlayer();
            }

            _priorStoryUnits.Clear();
            _groupID = groupID;
            _storyUnits = units;
            _currentUnitIndex = 0;
            _currentStoryUnit = units[_currentUnitIndex];
            IsLocking = false;
            IsPlaying = true;
            IsExecutable = true;
        }
        public void ExecuteCurrentUnit(bool breakLock = false)
        {
            // �ı�����
            if (_isPlaying && UIModule.IsPrintingText && !IsExecutable)
            {
                UIModule.Skip();
                if (IsAuto)
                {
                    IsAuto = false;
                }
                return;
            }
            // ������ ��Է�Autoģʽ
            if (IsLocking && !IsAuto && !breakLock)
            {
                return;
            }
            if (!IsExecutable || !_isPlaying)
            {
                return;
            }
            if (_currentStoryUnit == null)
            {
                CloseStoryPlayer();
                return;
            }

            // ÿһ����Ԫˢ��һ������ʱ��
            ReflashLockTime();

            if (!string.IsNullOrEmpty(_currentStoryUnit.scripts))
            {
                ScriptsToExecute.Add(_currentStoryUnit.scripts);
            }

            switch (_currentStoryUnit.type)
            {
                case UnitType.Text:
                case UnitType.Title:
                    {
                        _currentStoryUnit.Execute();
                        PrepareNextStoryUnit();
                        IsExecutable = false;
                        break;
                    }
                case UnitType.Option:
                    {
                        _currentStoryUnit.Execute();
                        IsExecutable = false;
                        break;
                    }
                case UnitType.Command:
                    {
                        if (_currentStoryUnit.wait != 0)
                        {
                            // ���ݵ�Ԫ�ȴ�ʱ��ִ�еȴ���Ϻ��Զ�ִ����һ��Ԫ
                            _currentStoryUnit.Execute();
                            IsExecutable = false;
                            this.Delay(() =>
                           {
                               IsExecutable = true;
                               PrepareNextStoryUnit();
                               ExecuteCurrentUnit(true);
                           }, _currentStoryUnit.wait / 1000f);
                            break;
                        }
                        else
                        {
                            _currentStoryUnit.Execute();
                            PrepareNextStoryUnit();
                            ExecuteCurrentUnit(true);
                            break;
                        }

                    }
                default:
                    break;
            }
        }
        public void PrepareNextStoryUnit()
        {
            if(_priorStoryUnits.Count > 0)
            {
                _currentStoryUnit = _priorStoryUnits.Dequeue();
            }
            else
            {
                _currentUnitIndex++;
                if (_currentUnitIndex >= _storyUnits.Count)
                {
                    _currentStoryUnit = null;
                }
                else
                {
                    _currentStoryUnit = _storyUnits[_currentUnitIndex];
                }
            }
        }
        public void ReadyToExecute(bool canExecute = false)
        {
            IsExecutable = true;
            // ��Auto��ֱ��ִ��
            if (canExecute || IsAuto)
            {
                ExecuteCurrentUnit();
            }
        }

        public void CloseStoryPlayer(bool fadeOut = true, bool destoryObject = false)
        {
            if(!IsReadyToClosePlayer && !IsSkippable)
            {
                // TODO
                // ִ��ĳ�ֿ��������ߵ�����ʾ
                Debug.Log("���ڷ�֧����Ԫ�ű����޷����������Ǳ༭����");
#if !UNITY_EDITOR
                // return;
#endif
            }

            IsPlaying = false;
            AudioModule.PauseBGM();

            Action DoClosePlayer = () =>
            {
                EventBus<OnClosedStoryPlayer>.Raise();
                OnStoryPlayerClosed?.Invoke(ScriptsToExecute, ModifiedFlagTable);
                OnStoryPlayerClosed = null;
                if (!destoryObject)
                {
                    gameObject.SetActive(false);
                    BackgroundModule.SetBackground();
                    UIModule.HideAllUI();
                    ClearPreloadAsset();
                    DoTweenS.DoTweenS.KillAll();
                    AudioModule.ClearAll();
                }
                else
                {
                    Destroy(gameObject);
                }
            };

            this.Delay(() =>
           {
               if (fadeOut)
               {
                   RequireBackdrop(BackdropType.Out, 1, DoClosePlayer);
               }
               else
               {
                   DoClosePlayer();
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
        private void RequireBackdrop(BackdropType type,float duration,Action feedback = null)
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
                        image.DoAlpha(0, duration).onCompleted = ()=> { feedback?.Invoke(); Destroy(backdrop); };
                        break;
                    }
                case BackdropType.Out:
                    {
                        image.color = new Color(0, 0, 0, 0);
                        image.DoAlpha(1, duration).onCompleted = () => { feedback?.Invoke(); Destroy(backdrop); };
                        break;
                    }
                case BackdropType.OutIn:
                    {
                        image.color = new Color(0, 0, 0, 0);
                        image.DoAlpha(1, duration/2).onCompleted = () => { feedback?.Invoke(); image.DoAlpha(0, duration/2).onCompleted = ()=> { Destroy(backdrop); }; };
                        break;
                    }
                case BackdropType.InOut:
                    {
                        image.color = new Color(0, 0, 0, 1);
                        image.DoAlpha(0, duration/2).onCompleted = () => { feedback?.Invoke(); image.DoAlpha(1, duration/2).onCompleted = ()=> { Destroy(backdrop); }; };
                        break;
                    }
                default:return;

            }
        }

        /// <summary>
        /// ��������һ��ʱ�� ����ȷ�������������
        /// </summary>
        private void Lock(float duration,float extra = 0.2f)
        {
            if (duration + extra > _lockTime)
            {
                ReflashLockTime(duration + extra);
            }
            else
            {
                return;
            }

            if (IsLocking)
                StopCoroutine(_crtLock);

            IsLocking = true;
            _crtLock = this.Delay(() =>
            {
                IsLocking = false;
            }, duration + extra);
        }
        /// <summary>
        /// ˢ�µ�ǰ��Ԫ����ʱ��
        /// </summary>
        /// <param name="duration"></param>
        private void ReflashLockTime(float duration = 0)
        {
            _lockTime = duration;
        }

        private CharacterDataTable TryGetCharactetDatable()
        {
            return ScriptableObject.Instantiate(Resources.Load<CharacterDataTable>("CharacterDataTable"));
        }
        private PlayerSetting TryGetPlayerSetting()
        {
            var setting = Resources.Load<PlayerSetting>("PlayerSetting");
            if(setting == null)
            {
                Debug.LogWarning($"δ���ò������趨����ʹ��Ĭ�ϱ�");
                return ScriptableObject.CreateInstance<PlayerSetting>();
            }
            else
            {
                return ScriptableObject.Instantiate(setting);
            }
        }

        public void ClearPreloadAsset()
        {
            BackgroundModule.ClearPreloadedImages();
            AudioModule.ClearPreloadedMusicClips();
            CharacterModule.ClearAllObject();
            CharacterModule.EmotionFactory.ClearCache();
            CharacterDataTable.ClearRuntimeUnits();
        }

        private void OnSetCharacterActionEventHandler(OnSetCharacterAction data)
        {
            Lock(data.time, Setting.TimeLockAfterAction);
        }
        private void OnPlayerSelectedEventHandler(OnPlayerSelected data)
        {
            if (data.storyUnits != null && data.storyUnits.Count > 0)
            {
                foreach (StoryUnit priorUnit in data.storyUnits)
                {
                    _priorStoryUnits.Enqueue(priorUnit);
                }
            }
            if (!string.IsNullOrEmpty(data.script))
            {
                ScriptsToExecute.Add(data.script);
            }

            PrepareNextStoryUnit();
        }
        private void OnFinishedPrintingMainTextEventHandler(OnFinishedPrintingMainText data)
        {
            Lock(Setting.TimeLockAfterPrinting);
        }
    }
}
