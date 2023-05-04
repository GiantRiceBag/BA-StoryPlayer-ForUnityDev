using System.Collections.Generic;
using UnityEngine;

namespace BAStoryPlayer
{
    [System.Serializable]
    public class CharacterDataUnit
    {
        [Tooltip("由于目前使用场景的角色量不多 以角色罗马音作为主要查询依据")]
        public string romaji;
        public string familyName;
        public string name;
        public string collage;
        public string affiliation;
        public string prefabUrl;
        public string portraitUrl;

        public override string ToString()
        {
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            result.Append($"罗马音 {romaji}\n");
            result.Append($"姓名 {familyName}{name}\n");
            result.Append($"学院 {collage}\n");
            result.Append($"所属社团 {affiliation}");
            return result.ToString() ;
        }
    }

    [CreateAssetMenu(menuName = "BAStoryPlayer/CharacterDataLookupTable",fileName = "CharacterDataLookupTable")]
    public class CharacterData : ScriptableObject
    {
        [SerializeField]
        List<CharacterDataUnit> Datas = new List<CharacterDataUnit>();

        public CharacterDataUnit this[string romaji]
        {
            get
            {
                foreach(var i in Datas)
                {
                    if (i.romaji == romaji)
                        return i;
                }
                Debug.LogError($"未能在查询表中找到 角色[{romaji}] 的数据");
                return null;
            }
        }

        public void Print()
        {
            foreach(var i in Datas)
            {
                Debug.Log(i.ToString());
            }
        }
    }
}

