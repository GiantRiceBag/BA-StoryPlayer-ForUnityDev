using BAStoryPlayer.NexonScriptParser;
using System.Collections.Generic;

namespace BAStoryPlayer
{
    public class NexonScriptData
    {
        public string script;
        public ScriptTag tag;

        public NexonScriptData(string script, ScriptTag tag)
        {
            this.script = script;
            this.tag = tag;
        }
    }

    [System.Serializable]
    public class NexonStoryScript
    {
        [UnityEngine.SerializeField] int groupID;
        [UnityEngine.SerializeField] List<RawNexonStoryUnit> content = new List<RawNexonStoryUnit>();

        public int GroupID => groupID;
        public List<RawNexonStoryUnit> Content => content;

        public void Print()
        {
            UnityEngine.Debug.Log($"��ID : {GroupID}");
            UnityEngine.Debug.Log($"��Ԫ�� : {Content.Count}");
            foreach (var i in Content)
                UnityEngine.Debug.Log(i.ToString());
        }

        public void ToJsonFile(string url)
        {
            System.IO.StreamWriter sw = new System.IO.StreamWriter(UnityEngine.Application.dataPath + $"/{url}");
            sw.Write(ToJson());
            sw.Close();
        }
        public string ToJson()
        {
            return UnityEngine.JsonUtility.ToJson(this, true);
        }
    }

    [System.Serializable]
    public class RawNexonStoryUnit
    {
        public string scripts;
        public int selectionGroup;
        public string bgmURL;
        public string soundURL;
        public string backgroundURL;
        public string popupFileURL;
        [System.NonSerialized] public List<NexonScriptData> scriptList = new List<NexonScriptData>(); // ��������

#if UNITY_EDITOR
        public override string ToString()
        {
            System.Text.StringBuilder text = new System.Text.StringBuilder();
            text.Append($"�ű� : {scripts}\n");
            text.Append($"ѡ���� : {selectionGroup}\n");
            text.Append($"��������URL : {bgmURL}\n");
            text.Append($"��ЧURL : {soundURL}\n");
            text.Append($"����URL : {backgroundURL}\n");
            text.Append($"�ļ�URL : {popupFileURL}");
            return text.ToString();
        }
#endif
    }
}
