namespace BAStoryPlayer
{
    public enum UnitType
    {
        Text = 0,
        Command,
        Option,
        Title,
    }

    public class StoryUnit
    {
        public UnitType type = UnitType.Text;
        public System.Action action;
        public float wait = 0;
        public int selectionGroup = -1;

        public void Execute()
        {
            action();
        }
    }

}
