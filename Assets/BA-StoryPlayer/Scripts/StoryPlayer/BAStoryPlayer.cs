using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
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
        private Queue<StoryUnit> _priorStoryUnits = new(); // 优先单元
        [SerializeField] private float _lockTime = 0;

        [SerializeField] private bool _isPlaying = false;
        [SerializeField] private bool _isExecutable = true;
        [SerializeField] private bool _isLocking = false;

        private Coroutine _crtLock;

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
                if (_isAuto && _isPlaying && IsExecutable) // 文本输出完毕后的状态
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
                        Debug.LogError($"未配置角色信息表!");
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

        private void Start()
        {
            FlagTable = new();
            ModifiedFlagTable = new();
            ScriptsToExecute = new();
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

        public bool LoadStory(string url,Dictionary<string,int> flagTable = null)
        {
            if (IsPlaying)
            {
                Debug.Log("剧情播放中");
                return false;
            }

            var textAsset = Resources.Load<TextAsset>(Setting.PathStoryScript + url);
            if (textAsset == null)
            {
                Debug.LogError($"未能在 {Setting.PathStoryScript + url} 找到剧情脚本");
                return false;
            }

            FlagTable = flagTable;
            ModifiedFlagTable.Clear();
            ScriptsToExecute.Clear();

            IsPlaying = true;
            gameObject.SetActive(true);

            CommandParser parser = new UniversalCommandParser(this);

            LoadUnits(0, parser.Parse(textAsset));
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
            // 文本跳过
            if (_isPlaying && UIModule.IsPrintingText && !IsExecutable)
            {
                UIModule.Skip();
                if (IsAuto)
                {
                    IsAuto = false;
                }
                return;
            }
            // 操作锁 针对非Auto模式
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

            // 每一个单元刷新一次锁定时间
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
                            // 根据单元等待时间执行等待完毕后自动执行下一单元
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
            // 若Auto则直接执行
            if (canExecute || IsAuto)
            {
                ExecuteCurrentUnit();
            }
        }

        public void CloseStoryPlayer(bool fadeOut = true, bool destoryObject = false)
        {
            _isPlaying = false;
            AudioModule.PauseBGM();

            this.Delay(() =>
           {
               if (fadeOut)
               {
                   RequireBackdrop(BackdropType.Out, 1, () =>
                   {
                       EventBus<OnClosedStoryPlayer>.Raise();
                       OnStoryPlayerClosed?.Invoke(ScriptsToExecute, ModifiedFlagTable);
                       OnStoryPlayerClosed = null;
                       CharacterModule.EmotionFactory.ClearCache();
                       if (!destoryObject)
                       {
                           gameObject.SetActive(false);
                           BackgroundModule.SetBackground();
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
                   OnStoryPlayerClosed?.Invoke(ScriptsToExecute,ModifiedFlagTable);
                   OnStoryPlayerClosed = null;
                   CharacterModule.EmotionFactory.ClearCache();
                   if (!destoryObject)
                   {
                       gameObject.SetActive(false);
                       BackgroundModule.SetBackground();
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
        /// 锁定操作一段时间 用于确保动作播放完毕
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
        /// 刷新当前单元锁定时间
        /// </summary>
        /// <param name="duration"></param>
        private void ReflashLockTime(float duration = 0)
        {
            _lockTime = duration;
        }

        private CharacterDataTable TryGetCharactetDatable()
        {
            return Resources.Load<CharacterDataTable>("CharacterDataTable");
        }
        private PlayerSetting TryGetPlayerSetting()
        {
            var setting = Resources.Load<PlayerSetting>("PlayerSetting");
            if(setting == null)
            {
                Debug.LogWarning($"未引用播放器设定表，已使用默认表。");
                return ScriptableObject.CreateInstance<PlayerSetting>();
            }
            else
            {
                return ScriptableObject.Instantiate(setting);
            }
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
