using UnityEngine;

namespace BAStoryPlayer
{
    [System.Serializable]
    public class CharacterDataUnit
    {
        [Tooltip("由于目前使用场景的角色量不多 以角色罗马音作为主要索引名")]
        public string indexName;
        public string familyName;
        public string name;
        public string collage;
        public string affiliation;
        public string prefabUrl;
        public string portraitUrl;

        public override string ToString()
        {
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            result.Append($"索引名 {indexName}\n");
            result.Append($"姓名 {familyName}{name}\n");
            result.Append($"学院 {collage}\n");
            result.Append($"所属社团 {affiliation}");
            return result.ToString() ;
        }
    }

    [CreateAssetMenu(menuName = "BAStoryPlayer/角色数据表",fileName = "CharacterDataTable")]
    public class CharacterData : ScriptableObject
    {
        [SerializeField]
        System.Collections.Generic.List<CharacterDataUnit> Datas = new System.Collections.Generic.List<CharacterDataUnit>();

        public CharacterDataUnit this[string indexName]
        {
            get
            {
                foreach(var i in Datas)
                {
                    if (i.indexName == indexName)
                        return i;
                }
                Debug.LogError($"未能在查询表中找到 角色[{indexName}] 的数据");
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

