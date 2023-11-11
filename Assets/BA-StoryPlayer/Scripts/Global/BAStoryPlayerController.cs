using UnityEngine;
using BAStoryPlayer.NexonScriptParser;
using BAStoryPlayer.UniversaScriptParser;
using BAStoryPlayer.AsScriptParser;
using BAStoryPlayer.Event;

namespace BAStoryPlayer
{
    public enum StoryScriptType
    {
        Universal,
        Nexon,
        As
    }

    public class BAStoryPlayerController : BSingleton<BAStoryPlayerController>
    {
        private const string Path_Setting = "Setting/";
        private const string Path_StoryPlayer = "StoryPlayer";

       [Header("References")]
       [SerializeField] private CharacterData characterDataTable;
       [SerializeField] private PlayerSetting playerSetting;
       [SerializeField] private BAStoryPlayer storyPlayer;

        private bool isPlaying = false;

        public bool IsPlaying => isPlaying;

        public BAStoryPlayer StoryPlayer
        {
            get
            {
                if (storyPlayer == null)
                {
                    storyPlayer = FindObjectOfType<BAStoryPlayer>();

                    if (storyPlayer == null)
                    {
                        storyPlayer = Instantiate(Resources.Load(Path_StoryPlayer) as GameObject).GetComponent<BAStoryPlayer>();
                        storyPlayer.transform.SetParent(transform);
                        storyPlayer.name = "StoryPlayer";
                        storyPlayer.transform.localPosition = Vector3.zero;
                        storyPlayer.transform.localScale = Vector3.one;
                    }
                }

                return storyPlayer;
            }
        }
        public CharacterData CharacterDataTable
        {
            get
            {
                if (characterDataTable == null)
                {
                    // TODO 日后引入较为全面的学生数据表 --> 算了 引入的P 就那么点人 手输得了
                    characterDataTable = Resources.Load<CharacterData>(Path_Setting + "CharacterDataTable");
                    Debug.LogWarning($"未配置角色信息表!");
                }

                return characterDataTable;
            }
        }
        public PlayerSetting Setting
        {
            get
            {
                if (playerSetting == null)
                {
                    if (Application.isEditor)
                        return ScriptableObject.CreateInstance<PlayerSetting>();
                    Debug.LogWarning($"未引用播放器设定表 已使用默认表");
                    playerSetting = ScriptableObject.CreateInstance<PlayerSetting>();
                }

                return playerSetting;
            }
        }

        public BAStoryPlayer LoadStory(string url,StoryScriptType type = StoryScriptType.Universal)
        {
            if (isPlaying)
            {
                Debug.Log("剧情播放中");
                return null;
            }

            // TODO 不删除方案的选项
            StoryPlayer.gameObject.SetActive(true);

            
            var textAsset = Resources.Load<TextAsset>(Setting.Path_StoryScript + url);
            if(textAsset == null)
            {
                Debug.LogError($"未能在 {Setting.Path_StoryScript + url } 找到剧情脚本");
                return null;
            }

            ICommandParser parser = null;
            switch (type)
            {
                case StoryScriptType.Universal:
                    parser = new UniversalCommandParser();
                    break;
                case StoryScriptType.Nexon:
                    parser = new NexonCommandParser();
                    break;
                case StoryScriptType.As:
                    parser = new AsCommandParser();
                    break;
            }

            StoryPlayer.LoadUnits(0, parser.Parse(textAsset));
            StoryPlayer.ReadyToNext();
            StoryPlayer.Next();

            isPlaying = true;
            StoryPlayer.gameObject.SetActive(true);

            EventBus<OnClosedStoryPlayer>.ClearCallback();

            // 订阅播放结束事件
            EventBus<OnClosedStoryPlayer>.AddCallback(() =>
            {
                isPlaying = false;
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
