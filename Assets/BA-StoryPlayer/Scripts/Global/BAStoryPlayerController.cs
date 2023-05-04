using UnityEngine;

namespace BAStoryPlayer
{
    public class BAStoryPlayerController : BSingleton<BAStoryPlayerController>
    {
        const string PATH_CHARACTER_PREFABS = "CharacterPrefab/";
        const string PATH_STORYPLAYER = "StoryPlayer";
        const string PATH_CHARACTERDATA = "Setting/";

        CharacterData _characterDataTable;
        BAStoryPlayer _storyPlayer;

        public BAStoryPlayer StoryPlayer
        {
            get
            {
                if (_storyPlayer == null)
                    _storyPlayer = transform.Find("StoryPlayer").GetComponent<BAStoryPlayer>();
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
