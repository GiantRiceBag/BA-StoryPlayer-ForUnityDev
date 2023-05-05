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
                        Debug.LogError($"δ����·�� {PATH_CHARACTERDATA} �ҵ���Ϣ�����ֶ������Լ����� *ע��*��ʹ�ò�ѯ��Ĭ����");
                }

                return _characterDataTable;
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

            StoryScript storyScript;
            string json = Resources.Load<TextAsset>(PATH_STORYSCRIPT+url).ToString();
            storyScript = JsonUtility.FromJson(json, typeof(StoryScript)) as StoryScript;

            MasterParser parser = new MasterParser(); // ʵ����������
            
            StoryPlayer.LoadUnits(storyScript.groupID, parser.Parse(storyScript)); // ��������ʼ��
            StoryPlayer.Next(); // ��ʼ���ŵ�һ��ִ�е�Ԫ

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
                Debug.LogError($"δ��·��{PATH_CHARACTER_PREFABS + CharacterDataTable[romaji].prefabUrl}���ҵ���ɫԤ����");
                return null;
            }
            GameObject obj = Instantiate(prefab);
            return obj;
        }
    }
}
