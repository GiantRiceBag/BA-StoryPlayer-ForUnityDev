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
        [Header("背景设置")]
        [Tooltip("背景模糊时间")]
        [SerializeField] private float _timeBlurBackground = 0.7f;
        [Tooltip("背景切换过渡时间")]
        [SerializeField] private float _timeSwitchBackground = 1f;
        [Space]
        [Header("角色操作设置")]
        [Tooltip("角色渐入渐出时间")]
        [SerializeField] private float _timeCharacterFade = 0.75f;
        [Tooltip("高亮角色时间")]
        [SerializeField] private float _timeCharacterHighlight = 0.2f;
        [Tooltip("角色眨眼时间间隔范围(范围间随机值)")]
        [SerializeField] private Vector2 _timeRangeCharacterWink = new Vector2(4, 5.5f);
        [Tooltip("角色动作 Hophop/跳跃两次 所需时间")]
        [SerializeField] private float _timeCharacterHophop = 0.5f;
        [Tooltip("角色动作 Shake/摇晃 所需时间")]
        [SerializeField] private float _timeCharacterShake = 0.64f;
        [Tooltip("角色动作 Move/位置变换 所需时间")]
        [SerializeField] private float _timeCharacterMove = 0.45f;
        [Tooltip("角色动作 Stiff/小幅度剧烈摇晃 所需时间")]
        [SerializeField] private float _timeCharacterStiff = 0.45f;
        [Tooltip("角色动作 Jump/跳跃 所需时间")]
        [SerializeField] private float _timeCharacterJump = 0.3f;
        [Tooltip("角色动作 Greeting/向下移动并返回 所需时间")]
        [SerializeField] private float _timeCharacterGreeting = 0.8f;
        [Space]
        [Header("音乐音效设置")]
        [Tooltip("音效Source池最大数量")]
        [SerializeField] private int _numMaxAudioSource = 10;
        [Tooltip("背景音乐切换淡入淡出耗时(淡入淡出总耗时为该值两倍)")]
        [SerializeField] private float _timeBgmFade = 0.5f;
        [Space]
        [Header("资源路径设置")]
        [Tooltip("音乐资源路径"), HideInInspector]
        [SerializeField] private string _pathMusic = "Music/";
        [Tooltip("音效资源路径"), HideInInspector]
        [SerializeField] private string _pathSound = "Sound/";
        [Tooltip("背景图片路径"), HideInInspector]
        [SerializeField] private string _pathBackground = "Background/";
        [Tooltip("角色Skel以及Prefab路径"), HideInInspector]
        [SerializeField] private string _pathCharacterSkeletonData = "CharacterSkeletonData/";
        [Tooltip("剧情脚本路径"), HideInInspector]
        [SerializeField] private string _pathStoryScript = "StoryScript/";

        public int NumCharPerSecond => _numCharPerSecond;
        public float IntervalPrint => 1 / (float)_numCharPerSecond;
        public float TimeLockAfterPrinting => _timeLockAfterPrinting;
        public float TimeLockAfterAction => _timeLockAfterAction;

        public float TimeBlurBackground => _timeBlurBackground;
        public float TimeSwitchBackground => _timeSwitchBackground;

        public float TimeCharacterFade => _timeCharacterFade;
        public float TimeCharacterHighlight => _timeCharacterHighlight;
        public Vector2 TimeRangeCharacterWink => _timeRangeCharacterWink;
        public float TimeCharacterHophop => _timeCharacterHophop;
        public float TimeCharacterShake => _timeCharacterShake;
        public float TimeCharacterMove => _timeCharacterMove;
        public float TimeCharacterStiff => _timeCharacterStiff;
        public float TimeCharacterJump => _timeCharacterJump;
        public float TimeCharacterGreeting => _timeCharacterGreeting;

        public int NumMaxAudioSource => _numMaxAudioSource;
        public float TimeBgmFade => _timeBgmFade;

        public string PathMusic => _pathMusic;
        public string PathSound => _pathSound;
        public string PathBackground => _pathBackground;
        public string PathCharacterSkeletonData => _pathCharacterSkeletonData;
        public string PathStoryScript => _pathStoryScript;
    }
}
