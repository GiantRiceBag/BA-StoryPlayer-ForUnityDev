using UnityEngine;

namespace BAStoryPlayer
{
    public enum LoadType
    {
        Prefab = 0,
        SkeletonData
    }

    [System.Serializable]
    public class CharacterDataUnit
    {
        [Tooltip("�����Խ�ɫ��������Ϊ��Ҫ������")]
        public string indexName;
        [HideInInspector] public string familyName;
        public string name;
        [HideInInspector] public string collage;
        public string affiliation;
        [Space]
        public LoadType loadType = LoadType.SkeletonData;
        public string skelUrl;
        [HideInInspector] public string portraitUrl;
            
        public override string ToString()
        {
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            result.Append($"������ {indexName}\n");
            result.Append($"���� {familyName}{name}\n");
            result.Append($"ѧԺ {collage}\n");
            result.Append($"�������� {affiliation}");
            return result.ToString() ;
        }
    }

    [CreateAssetMenu(menuName = "BAStoryPlayer/��ɫ��Ϣ��",fileName = "CharacterDataTable")]
    [SerializeField]
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
                Debug.LogError($"δ���ڲ�ѯ�����ҵ� ��ɫ [{indexName}] ������");
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

