namespace BAStoryPlayer
{
    public enum UnitType
    {
        Command = 0,
        Text,
        Option,
        Title,
    }

    public class StoryUnit
    {
        public UnitType type = UnitType.Command;
        public System.Action action;
        public int weight = int.MinValue;
        public float wait = 0;
        public int selectionGroup = 0;
        public string scripts;

        public void Execute() { action?.Invoke(); }
        /// <summary>
        /// 根据新权重值更新单元标签
        /// </summary>
        public void UpdateType(UnitType type)
        {
            if (type >= this.type)
            {
                this.type = type;
            } 
        }

#if UNITY_EDITOR
        public override string ToString()
        {
            System.Text.StringBuilder text = new System.Text.StringBuilder();
            text.Append($"type : {type}\n");
            text.Append($"weight : {weight}\n");
            text.Append($"action : { action == null }\n");
            text.Append($"wait : {wait}\n");
            text.Append($"selectionGroup : {selectionGroup}");
            return text.ToString();
        }
#endif
    }

}
