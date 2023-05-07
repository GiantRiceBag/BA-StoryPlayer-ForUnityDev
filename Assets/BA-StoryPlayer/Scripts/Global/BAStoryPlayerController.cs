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
        public void LoadStory(string url)
        {
            if (isPlaying) { Debug.Log("���鲥����"); return; }

            isPlaying = true;

            // TODO ��ɾ��������ѡ��
            StoryPlayer.gameObject.SetActive(true);

            StoryScript storyScript;
            string json = Resources.Load<TextAsset>(Setting.Path_StoryScript+url).ToString();
            storyScript = JsonUtility.FromJson(json, typeof(StoryScript)) as StoryScript;

            MasterParser parser = new MasterParser(); // ʵ����������
            
            StoryPlayer.LoadUnits(storyScript.groupID, parser.Parse(storyScript)); // ��������ʼ��
            StoryPlayer.Next(); // ��ʼ���ŵ�һ��ִ�е�Ԫ

            // ���Ĳ��Ž����¼�
            StoryPlayer.OnFinishPlaying.AddListener(() =>
            {
                isPlaying = false;
            });
        }
        public void LoadStory(StoryScript storyScript)
        {
            if (isPlaying) { Debug.Log("���鲥����"); return; }

            isPlaying = true;

            // TODO ��ɾ��������ѡ��
            StoryPlayer.gameObject.SetActive(true);

            MasterParser parser = new MasterParser(); // ʵ����������

            StoryPlayer.LoadUnits(storyScript.groupID, parser.Parse(storyScript)); // ��������ʼ��
            StoryPlayer.Next(); // ��ʼ���ŵ�һ��ִ�е�Ԫ

            // ���Ĳ��Ž����¼�
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
                Debug.LogError($"δ��·��{Setting.Path_Prefab + CharacterDataTable[indexName].prefabUrl}���ҵ���ɫԤ����");
                return null;
            }
            GameObject obj = Instantiate(prefab);
            return obj;
        }
    }
}
