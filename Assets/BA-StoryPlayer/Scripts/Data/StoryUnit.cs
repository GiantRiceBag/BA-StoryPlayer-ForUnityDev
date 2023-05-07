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
        /// ������Ȩ��ֵ���µ�Ԫ��ǩ
        /// </summary>
        public void UpdateType(int nWeight, UnitType type) { if (nWeight >= weight) { this.type = type; } }
        public override string ToString()
        {
            System.Text.StringBuilder text = new System.Text.StringBuilder();
            text.Append($"��Ԫ���� : {type}\n");
            text.Append($"Ȩ�� : {weight}\n");
            text.Append($"ִ�к�ȴ�ʱ�� : {wait}\n");
            text.Append($"ѡ���� : {selectionGroup}");
            return text.ToString();
        }
    }

}
