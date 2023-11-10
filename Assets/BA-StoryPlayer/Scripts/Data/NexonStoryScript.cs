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
        public int groupID;
        public List<RawNexonStoryUnit> content = new List<RawNexonStoryUnit>();

#if UNITY_EDITOR
        public void Print()
        {
            UnityEngine.Debug.Log($"��ID : {groupID}");
            UnityEngine.Debug.Log($"��Ԫ�� : {content.Count}");
            foreach (var i in content)
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
#endif
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

            text.AppendLine($"�ű� : {scripts}");
            text.AppendLine($"ѡ���� : {selectionGroup}");
            text.AppendLine($"��������URL : {bgmURL}");
            text.AppendLine($"��ЧURL : {soundURL}");
            text.AppendLine($"����URL : {backgroundURL}");
            text.AppendLine($"�ļ�URL : {popupFileURL}");

            return text.ToString();
        }
#endif
    }
}
