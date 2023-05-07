using UnityEngine;
namespace BAStoryPlayer
{
    [CreateAssetMenu(menuName = "BAStoryPlayer/播放器设定", fileName = "PlayerSetting")]
    public class PlayerSetting : ScriptableObject
    {
        [Header("每秒文本输出字数")]
        [SerializeField] int num_Char_Persecond = 20;
        [Header("当文本输出完毕时 锁定操作时间")]
        [SerializeField] float time_Lock_AfterPrinting = 0.5f;
        [Header("当角色动作执行完毕时 额外锁定操作时间")]
        [SerializeField] float time_Lock_AfterAction = 0.5f;
        [Space]
        [Header("背景模糊时间")]
        [SerializeField] float time_BlurBackground = 0.7f;
        [Header("背景切换过渡时间")]
        [SerializeField] float time_SwitchBackground = 1f;
        [Space]
        [Header("角色渐入渐出时间")]
        [SerializeField] float time_Character_Fade = 0.75f;
        [Header("高亮角色时间")]
        [SerializeField] float time_Character_Highlight = 0.2f;
        [Header("角色眨眼时间间隔范围(范围间随机值)")]
        [SerializeField] Vector2 time_Character_Wink = new Vector2(4, 5.5f);
        [Header("角色动作 Hophop/跳跃两次 所需时间")]
        [SerializeField] float time_Character_Hophop = 0.5f;
        [Header("角色动作 Shake/摇晃 所需时间")]
        [SerializeField] float time_Character_Shake = 0.64f;
        [Header("角色动作 Move/位置变换 所需时间")]
        [SerializeField] float time_Character_Move = 0.45f;
        [Header("角色动作 Stiff/小幅度剧烈摇晃 所需时间")]
        [SerializeField] float time_Character_Stiff = 0.45f;
        [Header("角色动作 Jump/跳跃 所需时间")]
        [SerializeField] float time_Character_Jump = 0.3f;
        [Header("角色动作 Greeting/向下移动并返回 所需时间")]
        [SerializeField] float time_Character_Greeting = 0.8f;
        [Space]
        [Header("音效Source池最大数量")]
        [SerializeField] int num_Max_AudioSource = 10;
        [Header("背景音乐切换淡入淡出耗时(淡入淡出总耗时为该值两倍)")]
        [SerializeField] float time_Bgm_Fade = 0.5f;
        [Space]
        [Header("音乐资源路径")]
        [SerializeField] string path_Music = "Music/";
        [Header("音效资源路径")]
        [SerializeField] string path_Sound = "Sound/";
        [Header("背景图片路径")]
        [SerializeField] string path_Background = "Background/";
        [Header("角色预制体路径")]
        [SerializeField] string path_Prefab = "CharacterPrefab/";
        [Header("剧情脚本路径")]
        [SerializeField] string path_StoryScript = "StoryScript/";

        public int Num_Char_Persecond { get { return num_Char_Persecond; } }
        public float Interval_Print { get { return 1 / (float) num_Char_Persecond; } }
        public float Time_Lock_AfterPrinting { get { return time_Lock_AfterPrinting; } }
        public float Time_Lock_AfterAction { get { return time_Lock_AfterAction; } }

        public float Time_BlurBackground { get { return time_BlurBackground; } }
        public float Time_SwitchBAckground { get { return time_SwitchBackground; } }
        
        public float Time_Character_Fade { get { return time_Character_Fade; } }
        public float Time_Character_Highlight { get { return time_Character_Highlight; } }
        public Vector2 Time_Character_Wink { get { return time_Character_Wink; } }
        public float Time_Character_Hophop { get { return time_Character_Hophop; } }
        public float Time_Character_Shake { get { return time_Character_Shake; } }
        public float Time_Character_Move { get { return time_Character_Move; } }
        public float Time_Character_Stiff { get { return time_Character_Stiff; } }
        public float Time_Character_Jump { get { return time_Character_Jump; } }
        public float Time_Character_Greeting { get { return time_Character_Greeting; } }

        public int Num_Max_AudioSource { get {return  num_Max_AudioSource; } }
        public float Time_Bgm_Fade { get { return time_Bgm_Fade; } }

        public string Path_Music { get { return path_Music; } }
        public string Path_Sound { get { return path_Sound; } }
        public string Path_Background { get { return path_Background; } }
        public string Path_Prefab { get { return path_Prefab; } }
        public string Path_StoryScript { get { return path_StoryScript; } }
    }
}
