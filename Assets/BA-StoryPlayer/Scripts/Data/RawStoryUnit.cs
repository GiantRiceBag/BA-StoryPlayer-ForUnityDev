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
        [System.NonSerialized]  public System.Collections.Generic.List<ScriptData> scriptList = new System.Collections.Generic.List<ScriptData>(); // ��������

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
    }
}   
