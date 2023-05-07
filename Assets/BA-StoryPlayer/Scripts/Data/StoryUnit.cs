namespace BAStoryPlayer
{
    public enum UnitType
    {
        Text,
        Command,
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

        public void Execute() { action?.Invoke(); }
        /// <summary>
        /// 根据新权重值更新单元标签
        /// </summary>
        public void UpdateType(int nWeight, UnitType type) { if (nWeight >= weight) { this.type = type; } }
        public override string ToString()
        {
            System.Text.StringBuilder text = new System.Text.StringBuilder();
            text.Append($"单元类型 : {type}\n");
            text.Append($"权重 : {weight}\n");
            text.Append($"执行后等待时间 : {wait}\n");
            text.Append($"选项组 : {selectionGroup}");
            return text.ToString();
        }
    }

}
