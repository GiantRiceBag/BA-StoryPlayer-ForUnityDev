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
                        Debug.LogError($"未能在路径 {PATH_CHARACTERDATA} 找到信息表请手动创建以及配置 *注意*请使用查询表默认名");
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
                Debug.LogError($"未在路径{PATH_CHARACTER_PREFABS + CharacterDataTable[romaji].prefabUrl}中找到角色预制体");
                return null;
            }
            GameObject obj = Instantiate(prefab);
            return obj;
        }
    }
}
