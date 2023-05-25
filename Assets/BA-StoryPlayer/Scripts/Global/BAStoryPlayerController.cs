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
                    // TODO �պ������Ϊȫ���ѧ�����ݱ�
                    characterDataTable = Resources.Load<CharacterData>(PATH_SETTING + "CharacterDataTable");
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
                    playerSetting = Resources.Load<PlayerSetting>(PATH_SETTING + "PlayerSetting");
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

            StoryScript storyScript;
            var textAsset = Resources.Load<TextAsset>(Setting.Path_StoryScript + url);
            if(textAsset == null)
            {
                Debug.LogError($"δ���� {Setting.Path_StoryScript + url } �ҵ�����ű�");
                return null;
            }

            isPlaying = true;

            string json = textAsset.ToString();
            storyScript = JsonUtility.FromJson(json, typeof(StoryScript)) as StoryScript;

            MasterParser parser = new MasterParser(); // ʵ����������
            
            StoryPlayer.LoadUnits(storyScript.groupID, parser.Parse(storyScript)); // ��������ʼ��
            StoryPlayer.Next(); // ��ʼ���ŵ�һ��ִ�е�Ԫ

            // ���Ĳ��Ž����¼�
            StoryPlayer.onFinishPlaying.AddListener(() =>
            {
                isPlaying = false;
            });

            return StoryPlayer;
        }
        public BAStoryPlayer LoadStory(StoryScript storyScript)
        {
            if (isPlaying) { Debug.Log("���鲥����"); return null; }
            
            isPlaying = true;

            // TODO ��ɾ��������ѡ��
            StoryPlayer.gameObject.SetActive(true);

            // �Ƴ��¼�
            StoryPlayer.onFinishPlaying.RemoveAllListeners();

            MasterParser parser = new MasterParser(); // ʵ����������

            StoryPlayer.LoadUnits(storyScript.groupID, parser.Parse(storyScript)); // ��������ʼ��
            StoryPlayer.Next(); // ��ʼ���ŵ�һ��ִ�е�Ԫ

            // ���Ĳ��Ž����¼�
            StoryPlayer.onFinishPlaying.AddListener(() =>
            {
                isPlaying = false;
            });

            return StoryPlayer;
        }

        public GameObject LoadCharacterPrefab(string indexName)
        {
            GameObject prefab = Resources.Load(Setting.Path_Prefab + CharacterDataTable[indexName].prefabUrl) as GameObject;
            prefab.name = indexName;
            if(prefab == null)
            {
                Debug.LogError($"δ��·��{Setting.Path_Prefab + CharacterDataTable[indexName].prefabUrl}���ҵ���ɫԤ����");
                return null;
            }
            GameObject obj = Instantiate(prefab);
            return obj;
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
