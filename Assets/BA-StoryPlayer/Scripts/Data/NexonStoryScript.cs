using System.Collections.Generic;

namespace BAStoryPlayer
{
    [System.Serializable]
    public class NexonStoryScript
    {
        [UnityEngine.SerializeField] int groupID;
        [UnityEngine.SerializeField] List<RawNexonStoryUnit> content = new List<RawNexonStoryUnit>();

        public int GroupID => groupID;
        public List<RawNexonStoryUnit> Content => content;

        public void Print()
        {
            UnityEngine.Debug.Log($"组ID : {GroupID}");
            UnityEngine.Debug.Log($"单元数 : {Content.Count}");
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
}
