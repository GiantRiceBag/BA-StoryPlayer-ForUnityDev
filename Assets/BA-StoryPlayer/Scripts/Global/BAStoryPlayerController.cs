using UnityEngine;
namespace BAStoryPlayer
{
    public class BAStoryPlayerController : BSingleton<BAStoryPlayerController>
    {
        const string PATH_STORYSCRIPT = "StoryScript/";
        const string PATH_CHARACTER_PREFABS = "CharacterPrefab/";
        const string PATH_CHARACTERDATA = "Setting/";

        const string PATH_STORYPLAYER = "StoryPlayer";

        CharacterData _characterDataTable;
       [SerializeField]  BAStoryPlayer _storyPlayer;

        public BAStoryPlayer StoryPlayer
        {
            get
            {
                if (_storyPlayer == null)
                {
                    var temp = transform.Find("StoryPlayer");
                    if(temp!=null)
                        temp.TryGetComponent<BAStoryPlayer>(out _storyPlayer);
                }
                if (_storyPlayer == null)
                {
                    _storyPlayer = Instantiate(Resources.Load(PATH_STORYPLAYER) as GameObject).GetComponent<BAStoryPlayer>();
                    _storyPlayer.transform.SetParent(transform);
                    _storyPlayer.name = "StoryPlayer";
                    _storyPlayer.transform.localPosition = Vector3.zero;
                    _storyPlayer.transform.localScale = Vector3.one;
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
                    _characterDataTable = Resources.Load<CharacterData>(PATH_CHARACTERDATA + "CharacterDataLookupTable");
                    if (_characterDataTable == null)
                        Debug.LogError($"未能在路径 {PATH_CHARACTERDATA} 找到信息表请手动创建以及配置 *注意*请使用查询表默认名");
                }

                return _characterDataTable;
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

            StoryScript storyScript;
            string json = Resources.Load<TextAsset>(PATH_STORYSCRIPT+url).ToString();
            storyScript = JsonUtility.FromJson(json, typeof(StoryScript)) as StoryScript;

            MasterParser parser = new MasterParser(); // 实例化解析器
            
            StoryPlayer.LoadUnits(storyScript.groupID, parser.Parse(storyScript)); // 播放器初始化
            StoryPlayer.Next(); // 开始播放第一个执行单元

            StoryPlayer.OnFinishPlaying.AddListener(() =>
            {
                isPlaying = false;
            });
        }

        public GameObject LoadCharacter(string romaji)
        {
            GameObject prefab = Resources.Load(PATH_CHARACTER_PREFABS + CharacterDataTable[romaji].prefabUrl) as GameObject;
            prefab.name = romaji;
            if(prefab == null)
            {
                Debug.LogError($"未在路径{PATH_CHARACTER_PREFABS + CharacterDataTable[romaji].prefabUrl}中找到角色预制体");
                return null;
            }
            GameObject obj = Instantiate(prefab);
            return obj;
        }
    }
}
