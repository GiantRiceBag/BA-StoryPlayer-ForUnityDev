using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using System;
using BAStoryPlayer.DoTweenS;
using BAStoryPlayer.UI;
using BAStoryPlayer.Event;
using Timer = BAStoryPlayer.Utility.Timer;
using BAStoryPlayer.Parser.UniversaScriptParser;

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
        [Space]
        [SerializeField] private CharacterDataTable _characterDataTable;
        [SerializeField] private PlayerSetting _playerSetting;

        private List<StoryUnit> _storyUnit;
        [Header("Real-Time Data")]
        [SerializeField] private int _currentUnitIndex = 0;
        private Queue<int> _priorUnitIndexes = new Queue<int>(); // 优先下标队列 
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
                    EventBus<OnPlayerCanceledAuto>.Raise();
                    EventBus<OnUnlockedPlayerInput>.ClearCallback();
                }
                if (_isAuto && _isPlaying && IsExecutable) // 文本输出完毕后的状态
                {
                    if (!IsLocking)
                        Next();
                    else
                        EventBus<OnUnlockedPlayerInput>.AddCallback(() =>
                        {
                            this.Delay(() =>
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
        public bool IsExecutable
        {
            get => _isExecutable;
            private set => _isExecutable = value;
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

        private void Start()
        {
            if (_imgBackground == null)
            {
                _imgBackground = transform.Find("Background").GetComponent<Image>();
            }

            // 动作以及表情事件订阅 锁定一定时间的操作
            EventBus<OnAnimatedCharacter>.Binding.Add((data) =>
            {
                Lock(data.time, Setting.TimeLockAfterAction);
            });
            // 选项事件订阅
            EventBus<OnPlayerSelectedBranch>.Binding.Add((data) =>
            {
                // 坐标前移寻找最近的选项下标 并放入优先下标队列 遇到0则停止
                for (int i = _currentUnitIndex; i < _storyUnit.Count; i++)
                {
                    if (_storyUnit[i].selectionGroup == data.selectionGroup)
                    {
                        _priorUnitIndexes.Enqueue(i);
                    }
                    else if (_storyUnit[i].selectionGroup == 0) // 注意添加最后一个无选项组的单元
                    {
                        _priorUnitIndexes.Enqueue(i);
                        break;
                    }
                }
                // 注意先切换下标
                NextIndex();
            });
            // 文本输出结束时间订阅 锁定操作一段时间
            EventBus<OnPrintedLine>.Binding.Add(() =>
            {
                Lock(Setting.TimeLockAfterPrinting);
            });
        }

        /// <summary>
        /// 设置背景 并优先适应宽度
        /// </summary>
        /// <param name="url">相对URL</param>
        /// <param name="type">背景切换方式 首次无效</param>
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
                        _imgBackground.DoColor(Color.black, Setting.TimeSwitchBackground).OnCompleted = () =>
                         {
                             _imgBackground.sprite = null;
                             _imgBackground.enabled = false;
                         };
                        break;
                }
                return;
            }

            Sprite sprite = Resources.Load<Sprite>(Setting.PathBackground + url);

            if (sprite == null)
            {
                Debug.LogError($"没能在路径 {Setting.PathBackground + url}  找到背景 [{url}]  ");
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
                            _imgBackground.DoColor(Color.black, Setting.TimeSwitchBackground / 2).OnCompleted = () =>
                            {
                                _imgBackground.sprite = sprite;
                                _imgBackground.DoColor(Color.white, Setting.TimeSwitchBackground / 2);
                            };
                            break;
                        }
                    default: return;
                }
            }
        }

        public bool LoadStory(string url)
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

            CommandParser parser = new UniversalCommandParser(this);

           LoadUnits(0, parser.Parse(textAsset));
            ReadyToNext();
            Next();

            IsPlaying = true;
            gameObject.SetActive(true);

            EventBus<OnClosedStoryPlayer>.ClearCallback();

            // 订阅播放结束事件
            EventBus<OnClosedStoryPlayer>.AddCallback(() =>
            {
                _isPlaying = false;
            });

            return true;
        }
        /// <summary>
        /// 载入执行单元 数据初始化
        /// </summary>
        private void LoadUnits(int groupID, List<StoryUnit> units)
        {
            _priorUnitIndexes.Clear();
            _groupID = groupID;
            _storyUnit = units;
            _currentUnitIndex = 0;
            IsLocking = false;
            IsPlaying = true;
            IsExecutable = true;
        }

        /// <summary>
        /// 执行下一单元
        /// </summary>
        private void Next(bool breakLock = false)
        {
            // 文本跳过
            if (_isPlaying && UIModule.IsPrintingText && !IsExecutable)
            {
                UIModule.Skip();
                if (IsAuto)
                    IsAuto = false;
                return;
            }

            // 操作锁 针对非Auto模式
            if (IsLocking && !IsAuto && !breakLock)
                return;

            if (!IsExecutable || !_isPlaying)
                return;

            if (_currentUnitIndex == _storyUnit.Count)
            {
                CloseStoryPlayer();
                return;
            }

            // 每一个单元刷新一次锁定时间
            ReflashLockTime();

            switch (_storyUnit[_currentUnitIndex].type)
            {
                case UnitType.Text:
                case UnitType.Title:
                case UnitType.Option:
                    {
                        _storyUnit[_currentUnitIndex].Execute();
                        NextIndex();
                        IsExecutable = false;
                        break;
                    }
                case UnitType.Command:
                    {
                        if (_storyUnit[_currentUnitIndex].wait != 0)
                        {
                            // 根据单元等待时间执行等待完毕后自动执行下一单元
                            _storyUnit[_currentUnitIndex].Execute();
                            IsExecutable = false;
                            this.Delay(() =>
                           {
                               IsExecutable = true;
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
        /// 加载下一个下标
        /// </summary>
        private void NextIndex()
        {
            if (_priorUnitIndexes.Count != 0)
            {
                _currentUnitIndex = _priorUnitIndexes.Dequeue();
            }
            else
                _currentUnitIndex++;
        }

        /// <summary>
        /// 准备执行下一单元
        /// </summary>
        /// <param name="next">是否直接执行下一单元</param>
        public void ReadyToNext(bool next = false)
        {
            IsExecutable = true;
            // 若Auto则直接执行
            if (next || IsAuto)
                Next();
        }

        /// <summary>
        /// 关闭播放器
        /// </summary>
        /// <param name="fadeOut">是否启用幕布渐出</param>
        /// <param name="destoryObject">是否删除播放器Object</param>
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
                       CharacterModule.EmotionFactory.ClearCache();
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
                   CharacterModule.EmotionFactory.ClearCache();
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
        /// 创建幕布
        /// </summary>
        /// <param name="type">幕布类型</param>
        /// <param name="duration">持续时间</param>
        /// <param name="feedback">回调函数</param>
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
                ReflashLockTime(duration + extra);
            else
                return;

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

        private void OnValidate()
        {
            
        }
    }
}
