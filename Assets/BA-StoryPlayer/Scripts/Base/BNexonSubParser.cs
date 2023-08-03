namespace BAStoryPlayer.NexonScriptParser
{
    public class BNexonSubParser
    {
        protected int weight = 0; // ����Ȩ��
        public BNexonSubParser nextParser;
        public virtual StoryUnit Parse(RawNexonStoryUnit rawStoryUnit, StoryUnit storyUnit = null) { return null; }
        public void SetNextParser(BNexonSubParser next) { nextParser = next; }
    }
}
