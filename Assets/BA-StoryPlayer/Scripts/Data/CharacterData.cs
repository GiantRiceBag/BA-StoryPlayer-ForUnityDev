using System.Collections.Generic;
using UnityEngine;

namespace BAStoryPlayer
{
    [System.Serializable]
    public class CharacterDataUnit
    {
        [Tooltip("����Ŀǰʹ�ó����Ľ�ɫ������ �Խ�ɫ��������Ϊ��Ҫ��ѯ����")]
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
            result.Append($"������ {romaji}\n");
            result.Append($"���� {familyName}{name}\n");
            result.Append($"ѧԺ {collage}\n");
            result.Append($"�������� {affiliation}");
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
                Debug.LogError($"δ���ڲ�ѯ�����ҵ� ��ɫ[{romaji}] ������");
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

