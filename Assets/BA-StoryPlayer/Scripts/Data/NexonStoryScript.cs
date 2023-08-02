namespace BAStoryPlayer
{
    [System.Serializable]
    public class NexonStoryScript
    {
        public int groupID;
        public System.Collections.Generic.List<RawNexonStoryUnit> content = new System.Collections.Generic.List<RawNexonStoryUnit>();

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
    }
}
