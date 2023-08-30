using UnityEngine;
using BAStoryPlayer.NexonScriptParser;
using BAStoryPlayer.AsScriptParser;
using BAStoryPlayer.Event;

namespace BAStoryPlayer
{
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

        public BAStoryPlayer LoadStory(string url)
        {
            if (isPlaying) { Debug.Log("剧情播放中"); return null; }

            // TODO 不删除方案的选项
            StoryPlayer.gameObject.SetActive(true);

            
            var textAsset = Resources.Load<TextAsset>(Setting.Path_StoryScript + url);
            if(textAsset == null)
            {
                Debug.LogError($"未能在 {Setting.Path_StoryScript + url } 找到剧情脚本");
                return null;
            }

            if(textAsset.text[0] == '{') // Nexon
            {
                string json = textAsset.ToString();
                NexonStoryScript storyScript = JsonUtility.FromJson(json, typeof(NexonStoryScript)) as NexonStoryScript;
                NexonCommandParser parser = new NexonCommandParser();
                StoryPlayer.LoadUnits(storyScript.GroupID, parser.Parse(storyScript)); 
                StoryPlayer.ReadyToNext();
                StoryPlayer.Next(); 
            }
            else // As
            {
                AsCommandParaser parser = new AsCommandParaser();
                var units = parser.Parse(textAsset);
                StoryPlayer.LoadUnits(0, units);
                StoryPlayer.ReadyToNext();
                StoryPlayer.Next(); 
            }

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

        public GameObject LoadCharacterPrefab(string indexName)
        {
            GameObject prefab = null;

            try
            {
                switch (CharacterDataTable[indexName].loadType)
                {
                    case LoadType.Prefab:
                        {
                            prefab = Instantiate(Resources.Load(Setting.Path_Prefab + CharacterDataTable[indexName].skelUrl) as GameObject);
                            break;
                        }
                    case LoadType.SkeletonData:
                        {
                            Spine.Unity.SkeletonDataAsset skelData = Resources.Load<Spine.Unity.SkeletonDataAsset>(Setting.Path_Prefab
                                + CharacterDataTable[indexName].skelUrl);
                            Material mat = new Material(Shader.Find("Spine/SkeletonGraphic"));
                            UnityEngine.Rendering.LocalKeyword keyword = new UnityEngine.Rendering.LocalKeyword(mat.shader, "_STRAIGHT_ALPHA_INPUT");
                            mat.SetKeyword(keyword, true);
                            prefab = Spine.Unity.SkeletonGraphic.NewSkeletonGraphicGameObject(skelData, transform, mat).gameObject;
                            break;
                        }
                }
            }
            catch(System.Exception e)
            {
                Debug.LogError(e.Message);
                return null;
            }

            prefab.name = indexName;
            return prefab;
        }

        //TEST
        public void LoadStoryTest(string url)
        {
            LoadStory(url);
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
