using UnityEngine;
using BAStoryPlayer.NexonScriptParser;
using BAStoryPlayer.AsScriptParser;

namespace BAStoryPlayer
{
    public class BAStoryPlayerController : BSingleton<BAStoryPlayerController>
    {
        const string Path_Setting = "Setting/";
        const string Path_StoryPlayer = "StoryPlayer";

       [Header("References")]
       [SerializeField] CharacterData characterDataTable;
       [SerializeField] PlayerSetting playerSetting;
       [SerializeField] BAStoryPlayer storyPlayer;

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
                    storyPlayer = Instantiate(Resources.Load(Path_StoryPlayer) as GameObject).GetComponent<BAStoryPlayer>();
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
                    // TODO �պ������Ϊȫ���ѧ�����ݱ�
                    characterDataTable = Resources.Load<CharacterData>(Path_Setting + "CharacterDataTable");
                    Debug.LogWarning($"δ���ý�ɫ��Ϣ��!");
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
                    playerSetting = new PlayerSetting();
                    Debug.LogWarning($"δ���ò������趨�� ��ʹ��Ĭ�ϱ�");
                }

                return playerSetting;
            }
        }

        bool isPlaying = false;

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="url">���½ű�URL Ĭ�Ϸ���Resources�µ�StoryScript�ļ�����</param>
        public BAStoryPlayer LoadStory(string url)
        {
            if (isPlaying) { Debug.Log("���鲥����"); return null; }

            // TODO ��ɾ��������ѡ��
            StoryPlayer.gameObject.SetActive(true);

            
            var textAsset = Resources.Load<TextAsset>(Setting.Path_StoryScript + url);
            if(textAsset == null)
            {
                Debug.LogError($"δ���� {Setting.Path_StoryScript + url } �ҵ�����ű�");
                return null;
            }

            if(textAsset.text[0] == '{') // Nexon
            {
                string json = textAsset.ToString();
                NexonStoryScript storyScript = JsonUtility.FromJson(json, typeof(NexonStoryScript)) as NexonStoryScript;
                NexonCommandParser parser = new NexonCommandParser();
                StoryPlayer.LoadUnits(storyScript.groupID, parser.Parse(storyScript)); 
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
            StoryPlayer.onFinishPlaying.RemoveAllListeners();

            // ���Ĳ��Ž����¼�
            StoryPlayer.onFinishPlaying.AddListener(() =>
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
