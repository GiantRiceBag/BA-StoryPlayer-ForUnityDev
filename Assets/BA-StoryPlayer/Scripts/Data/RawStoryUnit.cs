namespace BAStoryPlayer
{
    public class ScriptData
    {
        public string script;
        public ScriptTag tag;

       public ScriptData(string script,ScriptTag tag)
        {
            this.script = script;
            this.tag = tag;
        }
    }

    [System.Serializable]
    public class RawStoryUnit
    {
        public string scripts;
        public int selectionGroup;
        public string bgmURL;
        public string soundURL;
        public string backgroundURL;
        public string popupFileURL;
        [System.NonSerialized]  public System.Collections.Generic.List<ScriptData> scriptList = new System.Collections.Generic.List<ScriptData>(); // 供解析用

        public override string ToString()
        {
            System.Text.StringBuilder text = new System.Text.StringBuilder();
            text.Append($"脚本 : {scripts}\n");
            text.Append($"选项组 : {selectionGroup}\n");
            text.Append($"背景音乐URL : {bgmURL}\n");
            text.Append($"音效URL : {soundURL}\n");
            text.Append($"背景URL : {backgroundURL}\n");
            text.Append($"文件URL : {popupFileURL}");
            return text.ToString();
        }
    }
}   
