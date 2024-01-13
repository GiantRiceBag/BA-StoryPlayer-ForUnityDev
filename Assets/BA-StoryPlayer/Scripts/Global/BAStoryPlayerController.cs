using UnityEngine;

using BAStoryPlayer.Event;
using BAStoryPlayer.Parser.UniversaScriptParser;

namespace BAStoryPlayer
{
    public class BAStoryPlayerController : BSingleton<BAStoryPlayerController>
    {
        private const string SettingPath = "Setting/";
        private const string StoryPlayerPath = "StoryPlayer";

       [Header("References")]
       [SerializeField] private CharacterData _characterDataTable;
       [SerializeField] private PlayerSetting _playerSetting;
       [SerializeField] private BAStoryPlayer _storyPlayer;

        private bool _isPlaying = false;

        public bool IsPlaying => _isPlaying;

        public BAStoryPlayer StoryPlayer
        {
            get
            {
                if (_storyPlayer == null)
                {
                    _storyPlayer = FindObjectOfType<BAStoryPlayer>();

                    if (_storyPlayer == null)
                    {
                        _storyPlayer = Instantiate(Resources.Load(StoryPlayerPath) as GameObject).GetComponent<BAStoryPlayer>();
                        _storyPlayer.transform.SetParent(transform);
                        _storyPlayer.name = "StoryPlayer";
                        _storyPlayer.transform.localPosition = Vector3.zero;
                        _storyPlayer.transform.localScale = Vector3.one;
                    }
                }

                return _storyPlayer;
            }
        }
        public CharacterData CharacterDataTable
        {
            get
            {
                if (_characterDataTable == null)
                {
                    // TODO 日后引入较为全面的学生数据表 --> 算了 引入的P 就那么点人 手输得了
                    _characterDataTable = Resources.Load<CharacterData>(SettingPath + "CharacterDataTable");
                    Debug.LogWarning($"未配置角色信息表!");
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
                    if (Application.isEditor)
                        return ScriptableObject.CreateInstance<PlayerSetting>();
                    Debug.LogWarning($"未引用播放器设定表 已使用默认表");
                    _playerSetting = ScriptableObject.CreateInstance<PlayerSetting>();
                }

                return _playerSetting;
            }
        }

        public BAStoryPlayer LoadStory(string url)
        {
            if (_isPlaying)
            {
                Debug.Log("剧情播放中");
                return null;
            }

            // TODO 不删除方案的选项
            StoryPlayer.gameObject.SetActive(true);

            
            var textAsset = Resources.Load<TextAsset>(Setting.PathStoryScript + url);
            if(textAsset == null)
            {
                Debug.LogError($"未能在 {Setting.PathStoryScript + url } 找到剧情脚本");
                return null;
            }

            ICommandParser parser = new UniversalCommandParser();

            StoryPlayer.LoadUnits(0, parser.Parse(textAsset));
            StoryPlayer.ReadyToNext();
            StoryPlayer.Next();

            _isPlaying = true;
            StoryPlayer.gameObject.SetActive(true);

            EventBus<OnClosedStoryPlayer>.ClearCallback();

            // 订阅播放结束事件
            EventBus<OnClosedStoryPlayer>.AddCallback(() =>
            {
                _isPlaying = false;
            });

            return StoryPlayer;
        }
    }

    public static class ExtensionMethod
    {
        public static void ClearAllChild(this Transform transform)
        {
            for(int i = transform.childCount - 1; i >= 0; i--)
            {
                UnityEngine.GameObject.Destroy(transform.GetChild(i).gameObject);
            }
        }
    }
}
