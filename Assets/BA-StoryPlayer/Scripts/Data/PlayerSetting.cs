using System.IO;
using UnityEngine;
namespace BAStoryPlayer
{
    [CreateAssetMenu(menuName = "BAStoryPlayer/播放器设置", fileName = "PlayerSetting")]
    public class PlayerSetting : ScriptableObject
    {
        [Header("播放器设置")]
        [Tooltip("每秒文本输出字数")]
        [SerializeField] private int _numCharPerSecond = 20;
        [Tooltip("当文本输出完毕时 锁定操作时间")]
        [SerializeField] private float _timeLockAfterPrinting = 0.2f;
        [Tooltip("当角色动作执行完毕时 额外锁定操作时间")]
        [SerializeField] private float _timeLockAfterAction = 0.2f;
        [Space]
        [Header("过渡时间设置")]
        [Tooltip("背景模糊时间")]
        [SerializeField] private float _timeBlurBackground = 0.7f;
        [Tooltip("背景切换过渡时间")]
        [SerializeField] private float _timeSwitchBackground = 1f;
        [Space]
        [Header("角色操作设置")]
        [Tooltip("角色渐入渐出时间"), HideInInspector]
        [SerializeField] private float _defaultTimeCharacterFade = 0.75f;
        [Tooltip("高亮角色时间")]
        [SerializeField] private float _defaultTimeCharacterHighlight = 0.2f;
        [Tooltip("角色眨眼时间间隔范围(范围间随机值)")]
        [SerializeField] private Vector2 _defaultTimeRangeCharacterWink = new Vector2(4, 5.5f);
        [Tooltip("角色动作 Hophop/跳跃两次 所需时间"), HideInInspector]
        [SerializeField] private float _defaultTimeCharacterHophop = 0.5f;
        [Tooltip("角色动作 Shake/摇晃 所需时间"), HideInInspector]
        [SerializeField] private float _defaultTimeCharacterShake = 0.64f;
        [Tooltip("角色动作 Move/位置变换 所需时间"), HideInInspector]
        [SerializeField] private float _defaultTimeCharacterMove = 0.45f;
        [Tooltip("角色动作 Stiff/小幅度剧烈摇晃 所需时间"), HideInInspector]
        [SerializeField] private float _defaultTimeCharacterStiff = 0.45f;
        [Tooltip("角色动作 Jump/跳跃 所需时间"), HideInInspector]
        [SerializeField] private float _defaultTimeCharacterJump = 0.3f;
        [Tooltip("角色动作 Greeting/向下移动并返回 所需时间"), HideInInspector]
        [SerializeField] private float _defaultTimeCharacterGreeting = 0.8f;
        [Tooltip("角色动作 Falldown/左右摇摆后摔倒 所需时间"), HideInInspector]
        [SerializeField] private float _timeFalldown = 1.6f;
        [Space]
        [Header("音乐音效设置")]
        [Tooltip("音效Source池最大数量")]
        [SerializeField] private int _numMaxAudioSource = 10;
        [Tooltip("背景音乐切换淡入淡出耗时(淡入淡出总耗时为该值两倍)")]
        [SerializeField] private float _timeBgmFade = 0.5f;
        [Space]
        [Header("资源路径设置")]
        [Tooltip("音乐资源路径"), HideInInspector]
        [SerializeField] private string _pathMusic = "Music";
        [Tooltip("音效资源路径"), HideInInspector]
        [SerializeField] private string _pathSound = "Sound";
        [Tooltip("背景图片路径"), HideInInspector]
        [SerializeField] private string _pathBackground = "Background";
        [Tooltip("角色Skel以及Prefab路径"), HideInInspector]
        [SerializeField] private string _pathCharacterSkeletonData = "CharacterSkeletonData";
        [Tooltip("剧情脚本路径"), HideInInspector]
        [SerializeField] private string _pathStoryScript = "StoryScript";

        public int NumCharPerSecond => _numCharPerSecond;
        public float IntervalPrint => 1 / (float)_numCharPerSecond;
        public float TimeLockAfterPrinting => _timeLockAfterPrinting;
        public float TimeLockAfterAction => _timeLockAfterAction;

        public float TimeBlurBackground => _timeBlurBackground;
        public float TimeSwitchBackground => _timeSwitchBackground;

        public float DefaultTimeCharacterFade => _defaultTimeCharacterFade;
        public float DefaultTimeCharacterHighlight => _defaultTimeCharacterHighlight;
        public Vector2 DefaultTimeRangeCharacterWink => _defaultTimeRangeCharacterWink;
        public float DefaultTimeCharacterHophop => _defaultTimeCharacterHophop;
        public float DefaultTimeCharacterShake => _defaultTimeCharacterShake;
        public float DefaultTimeCharacterMove => _defaultTimeCharacterMove;
        public float DefaultTimeCharacterStiff => _defaultTimeCharacterStiff;
        public float DefaultTimeCharacterJump => _defaultTimeCharacterJump;
        public float DefaultTimeCharacterGreeting => _defaultTimeCharacterGreeting;
        public float DefaultTimeFalldown => _timeFalldown;

        public int NumMaxAudioSource => _numMaxAudioSource;
        public float TimeBgmFade => _timeBgmFade;

        public string PathMusic(string file) => Path.Combine(_pathMusic,file);
        public string PathSound(string file) => Path.Combine(_pathSound, file);
        public string PathBackground(string file) => Path.Combine(_pathBackground, file);
        public string PathCharacterSkeletonData(string file) => Path.Combine(_pathCharacterSkeletonData, file);
        public string PathStoryScript(string file) => Path.Combine(_pathStoryScript, file);
    }
}
