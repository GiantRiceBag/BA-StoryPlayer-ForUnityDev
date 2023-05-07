using UnityEngine;
namespace BAStoryPlayer
{
    public class BAStoryPlayerController : BSingleton<BAStoryPlayerController>
    {
        const string PATH_SETTING = "Setting/";
        const string PATH_STORYPLAYER = "StoryPlayer";

        [Header("References")]
       [SerializeField] CharacterData characterDataTable;
       [SerializeField] PlayerSetting playerSetting;
       [SerializeField]  BAStoryPlayer storyPlayer;

        public BAStoryPlayer StoryPlayer
        {
            get
            {
                if (storyPlayer == null)
                {
                    var temp = transform.Find("StoryPlayer");
                    if(temp!=null)
                        temp.TryGetComponent<BAStoryPlayer>(out storyPlayer);
                }
                if (storyPlayer == null)
                {
                    storyPlayer = Instantiate(Resources.Load(PATH_STORYPLAYER) as GameObject).GetComponent<BAStoryPlayer>();
                    storyPlayer.transform.SetParent(transform);
                    storyPlayer.name = "StoryPlayer";
                    storyPlayer.transform.localPosition = Vector3.zero;
                    storyPlayer.transform.localScale = Vector3.one;
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
                    // TODO 日后引入较为全面的学生数据表
                    characterDataTable = Resources.Load<CharacterData>(PATH_SETTING + "CharacterDataTable");
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
                    playerSetting = Resources.Load<PlayerSetting>(PATH_SETTING + "PlayerSetting");
                    Debug.LogWarning($"未引用播放器设定表 已使用默认表");
                }

                return playerSetting;
            }
        }

        bool isPlaying = false;

        /// <summary>
        /// 故事载入
        /// </summary>
        /// <param name="url">故事脚本URL 默认放于Resources下的StoryScript文件夹中</param>
        public void LoadStory(string url)
        {
            if (isPlaying) { Debug.Log("剧情播放中"); return; }

            isPlaying = true;

            // TODO 不删除方案的选项
            StoryPlayer.gameObject.SetActive(true);

            StoryScript storyScript;
            string json = Resources.Load<TextAsset>(Setting.Path_StoryScript+url).ToString();
            storyScript = JsonUtility.FromJson(json, typeof(StoryScript)) as StoryScript;

            MasterParser parser = new MasterParser(); // 实例化解析器
            
            StoryPlayer.LoadUnits(storyScript.groupID, parser.Parse(storyScript)); // 播放器初始化
            StoryPlayer.Next(); // 开始播放第一个执行单元

            // 订阅播放结束事件
            StoryPlayer.OnFinishPlaying.AddListener(() =>
            {
                isPlaying = false;
            });
        }
        public void LoadStory(StoryScript storyScript)
        {
            if (isPlaying) { Debug.Log("剧情播放中"); return; }

            isPlaying = true;

            // TODO 不删除方案的选项
            StoryPlayer.gameObject.SetActive(true);

            MasterParser parser = new MasterParser(); // 实例化解析器

            StoryPlayer.LoadUnits(storyScript.groupID, parser.Parse(storyScript)); // 播放器初始化
            StoryPlayer.Next(); // 开始播放第一个执行单元

            // 订阅播放结束事件
            StoryPlayer.OnFinishPlaying.AddListener(() =>
            {
                isPlaying = false;
            });
        }

        public GameObject LoadCharacter(string indexName)
        {
            GameObject prefab = Resources.Load(Setting.Path_Prefab + CharacterDataTable[indexName].prefabUrl) as GameObject;
            prefab.name = indexName;
            if(prefab == null)
            {
                Debug.LogError($"未在路径{Setting.Path_Prefab + CharacterDataTable[indexName].prefabUrl}中找到角色预制体");
                return null;
            }
            GameObject obj = Instantiate(prefab);
            return obj;
        }
    }
}
