using System;
using System.Collections.Generic;

using BAStoryPlayer.Parser.NexonScriptParser;

namespace BAStoryPlayer
{
    [Obsolete]
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

    [Serializable,Obsolete]
    public class NexonStoryScript
    {
        public int groupID;
        public List<RawNexonStoryUnit> content = new List<RawNexonStoryUnit>();

#if UNITY_EDITOR
        public void Print()
        {
            UnityEngine.Debug.Log($"组ID : {groupID}");
            UnityEngine.Debug.Log($"单元数 : {content.Count}");
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

    [Serializable,Obsolete]
    public class RawNexonStoryUnit
    {
        public string scripts;
        public int selectionGroup;
        public string bgmURL;
        public string soundURL;
        public string backgroundURL;
        public string popupFileURL;
        [System.NonSerialized] public List<NexonScriptData> scriptList = new List<NexonScriptData>(); // 供解析用

#if UNITY_EDITOR
        public override string ToString()
        {
            System.Text.StringBuilder text = new System.Text.StringBuilder();

            text.AppendLine($"脚本 : {scripts}");
            text.AppendLine($"选项组 : {selectionGroup}");
            text.AppendLine($"背景音乐URL : {bgmURL}");
            text.AppendLine($"音效URL : {soundURL}");
            text.AppendLine($"背景URL : {backgroundURL}");
            text.AppendLine($"文件URL : {popupFileURL}");

            return text.ToString();
        }
#endif
    }
}
